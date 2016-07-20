#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class AreaLight : MonoBehaviour
{
  public int FalloffIndex = 0;

  // Editor Variables
  public EAreaLightType Type;

  public bool HasEmitter = true;
  public bool IsTransparent = false;
  public bool IsDoubleSided = false;

  // Original Emission Texture
  public Texture2D EmissiveTexture;

  // Intensity
  public float Intensity
  {
    get { return GetComponent<Light>().intensity; }
    set { GetComponent<Light>().intensity = value; SetDirtyGlobal(); }
  }
  // Color
  public Color Color
  {
    get { return GetComponent<Light>().color; }
    set { GetComponent<Light>().color = value; SetDirtyGlobal(); }
  }

  public float Range = 10.0f;
  public float BounceIntensity = 1.0f;
  
  public int CullingMask
  {
    //TODO: set culling mask to light
    get { return GetComponent<Light>().cullingMask; }
    set { GetComponent<Light>().cullingMask = value; }
  }

  public Texture2D FalloffLookup
  {
    get { return _falloffLookup ?? (_falloffLookup = new Texture2D(m_texSize, 1, TextureFormat.RGB24, false, false)); }
    set { _falloffLookup = value; }
  }


  // runtime light index (global light)
  public int LightIndex;

  // for runtime changes
  protected AreaLightData m_runtime;
  private AreaLightData m_cached;

  // dirty flag (light needs update)
  private bool m_isDirty;
  public ELightUpdate m_needsUpdate;

  // emitter geometry
  protected AreaLightEmitter m_emitter;
  // enlighten light setup
  protected GameObject m_enlighten;
  private int m_enlightenUpdated;

  // 
  protected AreaLightManager m_lightMgr;

  // global update flag for light manager
  public static bool LightsNeedUpdate;

  public AnimationCurve FalloffCurve;

  private int m_texSize = 256;

  private Texture2D _falloffLookup;

  public AreaLightFalloffType FalloffType;

  // custom falloff index
  private float m_cfoIndex;

  public void Awake()
  {
    
  }

  public virtual void Start()
  {
    gameObject.hideFlags = HideFlags.None;
    m_lightMgr = AreaLightManager.Instance;
    m_lightMgr.AddLight(this);
    m_enlightenUpdated = 0;
    SetupEnlighten(true);
    SetDirty();

    _falloffLookup = new Texture2D(m_texSize, 1, TextureFormat.RGB24, false, false);
    UpdateFalloffLookup();
  }
  
  void OnEnable()
  {
    UpdateEmitter();
    SetDirty();
    GetComponent<Light>().hideFlags = HideFlags.HideInInspector;
  }

  void OnDestroy()
  {
    //LH: this causes a null ref exception on build
    if(m_lightMgr)
    {
      m_lightMgr.RemoveLight(this);
    }
    //m_lightMgr.RemoveLight(this);
    SetDirty();
  }

  public void Update()
  {
    if(m_lightMgr.UsedUpdateMode == AreaLightManager.UpdateMode.Update) DoUpdate();
  }

  public void FixedUpdate()
  {
    if (m_lightMgr.UsedUpdateMode == AreaLightManager.UpdateMode.FixedUpdate) DoUpdate();
  }

  public void LateUpdate()
  {
    if (m_lightMgr.UsedUpdateMode == AreaLightManager.UpdateMode.LateUpdate) DoUpdate();
  }

  public virtual void DoUpdate()
  {
    if (HasEmitter && m_emitter == null)
    {
      UpdateEmitter();
    }
    else if (!HasEmitter && m_emitter != null)
    {
      UpdateEmitter();
    }

    UpdateLight();
    UpdateEmitterMaterial();

    if (FalloffType == AreaLightFalloffType.Custom)
    {
      //UpdateFalloffLookup();
    }

    // check for enlighten update
    if (m_enlighten != null && !WantsGI())
    {
      m_enlighten.DestroyUniveral();
    }
    else if (m_enlighten == null && WantsGI())
    {
      SetupEnlighten(true);
    }
    //TODO: only update when we had a global light update?!
    if (m_enlighten)
    {
      UpdateEnlightenProxies();
    }

    if ((m_needsUpdate & ELightUpdate.Local) != 0)
    {
      RefreshCookieTexture();
      SetUpdated();
    }
  }

  public virtual void UpdateLight()
  {
    // update position and other stuff....
    var scale = transform.localScale;

    m_runtime.Width = Mathf.Abs(scale.x);
    m_runtime.Height = Mathf.Abs(scale.y);
    m_runtime.Radius = Mathf.Abs(Mathf.Max(scale.x, scale.y, scale.z));
    m_runtime.Range = Range;

    m_runtime.Location = transform.position;
    m_runtime.LocalRotation = transform.localRotation;
    m_runtime.LocalScale = transform.localScale;

    m_runtime.Type = Type;

    m_runtime.EmissiveTex = EmissiveTexture;

    // check for update
    m_needsUpdate |= m_runtime.NeedsUpdate(ref m_cached);

    // set global light update flag
    if ((m_needsUpdate & ELightUpdate.Global) != 0)
    {
      LightsNeedUpdate = true;
    }
  }

  public void UpdateFalloffLookup()
  {
    if (_falloffLookup == null)
    {
      _falloffLookup = new Texture2D(m_texSize, 1, TextureFormat.RGB24, false, false);
    }

    for (var x = 0; x < m_texSize; x++)
    {
      var idx = (float)x / (float)m_texSize;
      var intensity = x == m_texSize - 1 ? 0f : FalloffCurve.Evaluate(idx);
      for (var y = 0; y < 1; y++)
      {
        _falloffLookup.SetPixel(x, y, new Color(intensity, intensity, intensity, 1));
      }
    }

    _falloffLookup.Apply(false);
    _falloffLookup.wrapMode = TextureWrapMode.Clamp;

    SetDirty();

    m_lightMgr.CFONeedsUpdate = true;
  }

  public virtual void WriteLightCustomData(ref Texture2D lightLUT, int index, float packAdd, float packMult)
  {
    // to be overriden by subclasses
  }

  public void SetDirty()
  {
    m_needsUpdate = ELightUpdate.All;
    LightsNeedUpdate = true;
  }

  private void SetDirtyGlobal()
  {
    m_needsUpdate |= ELightUpdate.Global;
    LightsNeedUpdate = true;
  }

  protected virtual AreaLightEmitter GenerateEmitter()
  {
    return gameObject.AddComponent<AreaLightEmitter>();
  }

  public AreaLightEmitter GetEmitter()
  {
    if (m_emitter)
    {
      return m_emitter;
    }
    m_emitter = gameObject.GetComponent<AreaLightEmitter>();
    if (HasEmitter && m_emitter == null)
    {
      Debug.LogWarning("No valid emitter");
      UpdateEmitter();
    }
    return m_emitter;
  }

  public void UpdateEmitterMaterial()
  {
    if (gameObject.GetComponent<AreaLightEmitter>() != null)
    {
      gameObject.GetComponent<AreaLightEmitter>().IsTransparent = IsTransparent;
    }
  }

  public void UpdateEmitterCulling()
  {
    if (gameObject.GetComponent<AreaLightEmitter>() != null)
    {
      gameObject.GetComponent<AreaLightEmitter>().IsDoubleSided = IsDoubleSided;
      gameObject.GetComponent<AreaLightEmitter>().UpdateCullMode();
    }
  }

  public void UpdateEmitter()
  {
    if (HasEmitter)
    {
      if (m_emitter)
      {
        m_emitter.DestroyUniveral();
      }

      if (gameObject.GetComponent<AreaLightEmitter>() != null)
      {
        gameObject.GetComponent<AreaLightEmitter>().DestroyUniveral();
        m_emitter = null;
      }
      m_emitter = GenerateEmitter();
      
      if (Type == EAreaLightType.Disk || Type == EAreaLightType.Rectangular)
      {
        m_emitter.IsDoubleSided = IsDoubleSided;
        m_emitter.UpdateCullMode();
      }
    }
    else if (HasEmitter == false)
    {
      IsTransparent = false;
      if (m_emitter)
      {
        m_emitter.DestroyUniveral();
      }
    }
  }

  /**
   * Does this Lights wants global illumination
   */
  public bool WantsGI()
  {
    return m_lightMgr.GlobalIllumination && BounceIntensity > Mathf.Epsilon;
  }

  private void RefreshCookieTexture()
  {
    // write light index to cookie texture
    // TOD grab max lights from manager
    var lightIndex01 = (float)LightIndex / (float)10.0f;

    Texture2D internalCookie = null;

    if (EmissiveTexture)
    {
      try
      {
        var requiredWidth = EmissiveTexture.width;
        var requiredHeight = EmissiveTexture.height;

        var pixels = EmissiveTexture.GetPixels(0);

#if UNITY_ANDROID || UNITY_IOS
        TextureFormat texFormat = TextureFormat.ARGB32;
#else
        //float needed for iOS
        TextureFormat texFormat = TextureFormat.RGBAFloat;
#endif

        //emissive textures need mip maps
        internalCookie = new Texture2D(requiredWidth, requiredHeight, texFormat, true, true)
        {
          name = "light_cookie_dummy",
          /*mipMapBias = 0.125f,*/
          wrapMode = TextureWrapMode.Clamp,
          filterMode = FilterMode.Trilinear
        };

        /*
        // slow but working
        for (int i = 0; i < pixels.Length; ++i)
        {
          int x = i % requiredWidth;
          int y = i / requiredWidth;

          // black edge
          if (x == 0 || y == 0 || x == (requiredWidth - 1) || y == (requiredHeight - 1))
          {
            pixels[i].r = pixels[i].g = pixels[i].b = 0.0f;
          }

          internalCookie.SetPixel(x, y, new Color(pixels[i].r, pixels[i].g, pixels[i].b, lightIndex01));
          //internalCookie.SetPixel(x, y, pixels[i]);
        }
        */

        for (int i = 0; i < pixels.Length; ++i)
        {
          int x = i % requiredWidth;
          int y = i / requiredWidth;

          // black edge
          if (x == 0 || y == 0 || x == (requiredWidth - 1) || y == (requiredHeight - 1))
          {
            pixels[i] = new Color(0.0f, 0.0f, 0.0f, lightIndex01);
          }
          else
          {
            pixels[i] = new Color(pixels[i].r, pixels[i].g, pixels[i].b, lightIndex01);
          }
        }
        internalCookie.SetPixels(pixels);
        internalCookie.Apply();


        // fast but none working
        //internalCookie.SetPixels(pixels);
        //internalCookie.SetPixel(0, 0, new Color(pixels[0].r, pixels[0].g, pixels[0].b, lightIndex01));


        //var bytes = internalCookie.EncodeToPNG();
        //System.IO.File.WriteAllBytes(Application.dataPath + "/test_emissive_" + LightIndex + ".png", bytes);

        //FIXME: update emissive tex?
        m_runtime.EmissiveTex = EmissiveTexture;
        
      } catch(UnityException e) {
        Debug.LogWarning(e.Message);
        
        //float needed for iOS
        internalCookie = new Texture2D(1, 1, TextureFormat.RGBAFloat, false, true)
        {
          name = "light_cookie_dummy",
          wrapMode = TextureWrapMode.Clamp,
          filterMode = FilterMode.Point
        };

        internalCookie.SetPixel(0, 0, new Color(lightIndex01, lightIndex01, lightIndex01, lightIndex01));
        internalCookie.Apply();

        m_runtime.EmissiveTex = null;
      }
    }
    else
    {
      //float needed for iOS
      internalCookie = new Texture2D(1, 1, TextureFormat.RGBAFloat, false, true)
      {
        name = "light_cookie_dummy",
        wrapMode = TextureWrapMode.Clamp,
        filterMode = FilterMode.Point
      };

      internalCookie.SetPixel(0, 0, new Color(lightIndex01, lightIndex01, lightIndex01, lightIndex01));
      internalCookie.Apply();

      m_runtime.EmissiveTex = null;
    }

    // free old cookie
    if (m_runtime.Cookie && m_runtime.Cookie != internalCookie) {
      m_runtime.Cookie.DestroyUniveral();
    }

    m_runtime.Cookie = internalCookie;

    // set cookie texture for unity light
    GetComponent<Light>().cookie = m_runtime.Cookie;

    // debug output
    //Debug.Log("Updated Cookie Texture");
  }

  public virtual void CollectLightData(Texture2D lightLUT, int index)
  {
    m_needsUpdate |= m_runtime.NeedsUpdate(ref m_cached);

  }

  public void SetGlobalUpdated()
  {   
    // only update global values
    m_cached.UpdateValues(ELightUpdate.Global, ref m_runtime);
    m_needsUpdate &= ~ELightUpdate.Global;
  }

  // set this light to had been updated
  private void SetUpdated()
  {
    // this should make a copy
    //FIXME: only update local changes?!
    m_cached.UpdateValues(m_needsUpdate, ref m_runtime);
    m_needsUpdate = ELightUpdate.None;
  }

  // Callback for Enlighten Light Proxies
  public virtual void CreateEnlightenProxies()
  {

  }

  public virtual void UpdateEnlightenProxies()
  {

  }

  // recreates enlighten light setup
  public void SetupEnlighten(bool forceRenew)
  {
    // area lights are allowed to all this from update
    // to prevent a forever loop we check that we do not call this more than once
    // from update
    if (m_enlightenUpdated > 1)
      return;

#if UNITY_EDITOR
    // debug code if proxy is already generated -> this should not happen
    var existing = transform.FindChild("enlighten_proxy");
    if (existing)
    {
      m_enlighten = existing.gameObject;
    }
#endif

    if (m_enlighten != null && (BounceIntensity < Mathf.Epsilon || forceRenew || !m_lightMgr.GlobalIllumination))
    {
      m_enlighten.DestroyUniveral();
      m_enlighten = null;
    }

    if (BounceIntensity > Mathf.Epsilon && m_lightMgr.GlobalIllumination)
    {
      if (m_enlighten == null)
      {
        m_enlighten = new GameObject("enlighten_proxy");

        m_enlighten.transform.parent = transform;
        m_enlighten.transform.localPosition = new Vector3();
        m_enlighten.transform.localScale = new Vector3(1, 1, 1);
        m_enlighten.transform.localRotation = Quaternion.Euler(0, 0, 0);
        //TODO: add for production
        m_enlighten.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
        CreateEnlightenProxies();
      }

      m_enlightenUpdated++;
      UpdateEnlightenProxies();
      m_enlightenUpdated--;
    }
  }

  public virtual float SizeIntensity()
  {
    return 0.0f;
  }

  public virtual void OnDrawGizmos()
  {

  }
}
