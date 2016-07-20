using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class AreaLightManager : MonoBehaviour 
{
  public const int MaxLights = 64;
  public const string LUTShaderName = "_RedLightData";
  public const string FalloffShaderName = "_Falloff";

  // IES lookups
  public const string IESTextureShaderName = "_IESLUT";
  public const string IESCountShaderName = "_IESNumProfiles";

  // Custom FallOff - CFO
  public const string CFOTextureShaderName = "_CFO";
  public const string CFOCountShaderName = "_CFONum";

  public bool DebugView = false;
  public bool FastMode = false;
  public bool EarlyDiscard = false;

  public bool SpecNormalization = false;
  public bool GlobalIllumination = true;
  public UpdateMode UsedUpdateMode;
  private static AreaLightManager m_instance;

  [SerializeField]
  // Testing Variables
  public float SpecLightMultiplier
  {
    get { return m_specLightMultiplier; }
    set { m_specLightMultiplier = value; m_uploadShaderStates = true; }
  }
  public float DiffuseLightMultiplier
  {
    get { return m_diffuseLightMultiplier; }
    set { m_diffuseLightMultiplier = value; m_uploadShaderStates = true; }
  }

  [SerializeField]
  private float m_diffuseLightMultiplier = 1.0f;
  [SerializeField]
  private float m_specLightMultiplier = 1.0f;

  [SerializeField]
  private static List<AreaLight> m_lights;
  private static Texture2D m_lightLUT;

  private bool m_isCompiling;
  private bool m_lateUpdate;
  private bool m_uploadShaderStates;
  
  private Texture2D m_globalIES;
  private Texture2D m_globalFalloff;

  // custom fallofs
  private Texture2D m_globalCFO;
  public bool CFONeedsUpdate = false;

  // debug materials
  private Material m_matDebugLightLUT;
  private Material m_matDebugIESLUT;
  private Material m_matDebugCFOLUT;

  // Singleton Access
  public static AreaLightManager Instance
  {
    get
    {
      if (m_instance != null)
      {
        return m_instance;
      }

      m_instance = FindObjectOfType<AreaLightManager>();
      if (m_instance == null)
      {
        var go = new GameObject("redLights Manager");
        go.transform.position = Vector3.zero;
        go.transform.eulerAngles = Vector3.zero;
        go.transform.localScale = Vector3.one;

        m_instance = go.AddComponent<AreaLightManager>();

#if UNITY_EDITOR
        LayerUtility.InitLayer();
#endif
        InitList();
        InitLUT();
      }

      if (m_lights == null)
      {
        InitList();
      }

      DontDestroyOnLoad(m_instance);

      return m_instance;
    }
  }

  public void ForceLightsDirty()
  {
    m_lights.ForEach((l) => l.SetDirty());
  }

  public void HandlePlayModeChange()
  {
#if UNITY_EDITOR
    if (EditorApplication.isPlayingOrWillChangePlaymode)
    {
      ForceLightsDirty();
    }
    else
    {
      GatherLights();
    }
#endif
  }

  void OnEnable()
  {
#if UNITY_EDITOR
    EditorApplication.playmodeStateChanged += HandlePlayModeChange;
#endif
  }

  void OnDestroy()
  {
#if UNITY_EDITOR
    EditorApplication.update -= EditorUpdate;
    EditorApplication.playmodeStateChanged -= HandlePlayModeChange;
    m_instance = null;
#endif
  }

  void Start()
  {
    m_globalIES = Resources.Load("IES/IESLUT") as Texture2D;
    m_globalFalloff = Resources.Load("Textures/falloff_plotted") as Texture2D;
    m_uploadShaderStates = true;

#if UNITY_EDITOR
    EditorApplication.update += EditorUpdate;
#endif
  }

  // when in editor when not playing
  public void EditorUpdate()
  {
#if UNITY_EDITOR
    if (Application.isEditor && !Application.isPlaying)
    {
      if (m_lateUpdate)
      {
        m_lateUpdate = false;
        Update();
      }
    }
#endif
  }

  // game update lopp (play in editor also)
  public void Update()
  {
    if(DebugView)
    {
      InitDebugView();
    }
    else
    {
      DestroyDebugView();
    }

    if (m_lights == null)
    {
      InitList();
    }

    if (m_lightLUT == null)
    {
      InitLUT();
    }

#if UNITY_EDITOR
    if (m_isCompiling)
    {
      if (EditorApplication.isCompiling == false)
      {
        GatherLights();
        ForceLightsDirty();
      }
    }
    m_isCompiling = EditorApplication.isCompiling;
#endif

    // light table update
    if(AreaLight.LightsNeedUpdate)
    {
      // fill lights
      for (var i = 0; i < m_lights.Count; ++i)
      {
        if (m_lights[i] == null)
        {
          continue;
        }

        const float packAdd = 64.0f * 0.5f;
        const float packMult = 1.0f / 64.0f;

        // 0-1 range
        
        var dir = (m_lights[i].transform.forward.normalized + new Vector3(1, 1, 1)) * 0.5f;
        var right = (m_lights[i].transform.right.normalized + new Vector3(1, 1, 1)) * 0.5f;
        var up = (m_lights[i].transform.up.normalized + new Vector3(1, 1, 1)) * 0.5f;

        var index = i * 8;

        // write into buffer
        //TODO: add packing for RANGE
        m_lightLUT.SetPixel(index + 0, 0, new Color((float)m_lights[i].Type / 255.0f, packMult, m_lights[i].Range, m_lights[i].Intensity));
        m_lightLUT.SetPixel(index + 1, 0, Vector3ToColor(m_lights[i].transform.position));
        m_lightLUT.SetPixel(index + 2, 0, new Color(dir.x, dir.y, dir.z, 1.0f));
        m_lightLUT.SetPixel(index + 3, 0, new Color(right.x, right.y, right.z, 1.0f));
        m_lightLUT.SetPixel(index + 4, 0, new Color(up.x, up.y, up.z, 1.0f));

        // light specific data. 
        m_lights[i].WriteLightCustomData(ref m_lightLUT, index, packAdd, packMult);
        m_lights[i].LightIndex = i;

        if (m_lights[i].GetComponent<Light>())
        {
          var unityLight = m_lights[i].GetComponent<Light>();
          unityLight.renderMode = LightRenderMode.ForcePixel;
          unityLight.bounceIntensity = 0;
        }
        else
        {
          Debug.LogError("No valid Unity light");
        }

        m_lights[i].SetGlobalUpdated();
      }

      m_lightLUT.Apply();

      // reset global lights update
      AreaLight.LightsNeedUpdate = false;

      Shader.SetGlobalTexture(LUTShaderName, m_lightLUT);

      // debug output
      //Debug.Log("Update Global Light Table");
    }

    // check if we need a shader update
    if (m_uploadShaderStates)
    {
      Shader.SetGlobalTexture(FalloffShaderName, m_globalFalloff);
      Shader.SetGlobalTexture(IESTextureShaderName, m_globalIES);
      Shader.SetGlobalInt(IESCountShaderName, m_globalIES.height);

      Shader.SetGlobalFloat("_SpecLight_Multiplier", SpecLightMultiplier);
      Shader.SetGlobalFloat("_DiffuseLight_Multiplier", DiffuseLightMultiplier);

      //deferred rendering hack -> do we need this any more??
      //var lod = Camera.main.renderingPath == RenderingPath.DeferredShading ? 8.0f : 0.0f;
      //Shader.SetGlobalFloat("_LODOffset", lod);

      m_uploadShaderStates = false;
    }
  }

  public Vector4 Vector3ToColor(Vector3 pos)
  {
    // this is not ideal in any way
    //TODO: find packing range size
    const float PackRange = 1000.0f;

    // do this only once. somewhere appropriate
    Shader.SetGlobalFloat("_PackRange", PackRange);

    const float eps = 0.00001f;
    pos.x = Mathf.Abs(pos.x) < eps ? eps : pos.x;
    pos.y = Mathf.Abs(pos.y) < eps ? eps : pos.y;
    pos.z = Mathf.Abs(pos.z) < eps ? eps : pos.z;

    var absX = Mathf.Abs(pos.x);
    var absY = Mathf.Abs(pos.y);
    var absZ = Mathf.Abs(pos.z);

    var magn = Mathf.Max(absZ, Mathf.Max(absX, absY));
    var packedPos = new Vector4(pos.x / magn, pos.y / magn, pos.z / magn, magn / PackRange);
    packedPos = (packedPos + new Vector4(1, 1, 1, 1)) * 0.5f;

    return packedPos;
  }

  private static void InitList()
  {
    m_lights = new List<AreaLight>();
  }

  private static void InitLUT()
  {
    m_lightLUT = new Texture2D(512, 1, TextureFormat.RGBAFloat, false, true) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
    for (var x = 0; x < m_lightLUT.width; x++)
    {
      for (var y = 0; y < m_lightLUT.height; y++)
      {
        m_lightLUT.SetPixel(x, y, Color.white);
      }
    }
    m_lightLUT.Apply();
    Shader.SetGlobalTexture("_RedLightData", m_lightLUT);
  }

  public void RebuildCFO()
  {
    var height = (m_lights.Count + 1) * 3;
    var width = 256;

    m_globalCFO = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };

    var pixels = m_globalFalloff.GetPixels();
    m_globalCFO.SetPixels(0, 0, width, 1, pixels);
    m_globalCFO.SetPixels(0, 1, width, 1, pixels);
    m_globalCFO.SetPixels(0, 2, width, 1, pixels);

    for (var i = 0; i < m_lights.Count; i++)
    {
      var lightLUT = m_lights[i].FalloffLookup;
      pixels = lightLUT.GetPixels();

      m_globalCFO.SetPixels(0, (i + 1) * 3 + 0, width, 1, pixels);
      m_globalCFO.SetPixels(0, (i + 1) * 3 + 1, width, 1, pixels);
      m_globalCFO.SetPixels(0, (i + 1) * 3 + 2, width, 1, pixels);
    }

    m_globalCFO.Apply();

    Shader.SetGlobalTexture("_CFOLUT", m_globalCFO);
    Shader.SetGlobalInt("_CFONum", m_globalCFO.height);

    if (m_matDebugCFOLUT != null)
    {
      m_matDebugCFOLUT.SetTexture("_MainTex", m_globalCFO);
    }
  }

  public void LateUpdate()
  {
    if(CFONeedsUpdate)
    {
      CFONeedsUpdate = false;
      RebuildCFO();
    }
  }

  public void ClearLights()
  {
    m_lights.Clear();    
  }

  public void AddLight(AreaLight light)
  {
    if (m_lights.Contains(light))
    {
      return;
    }
    m_lights.Add(light);

    ResortList();

    //RebuildCFO();
    CFONeedsUpdate = true;
  }

  public void RemoveLight(AreaLight light)
  {
    if (m_lights == null)
    {
      InitList();
    }
    
    if (m_lights.Contains(light))
    {
      m_lights.Remove(light);
      ResortList();
    }

    //RebuildCFO();

    CFONeedsUpdate = true;
  }

  public void ResortList()
  {
    var tmp = new List<AreaLight>();
    m_lights.ForEach((l) =>
    {
      if (l != null)
      {
        tmp.Add(l);
        l.SetDirty();
      }
    });
    m_lights = tmp;
  }

  // finds objects through scene
  // only call this function if absolutly necessry gathering
  public void GatherLights()
  {
    var lights = Object.FindObjectsOfType<AreaLight>();

    ClearLights();
    foreach(var light in lights)
    {
      AddLight(light);
    }
    ResortList();

    // we do need a late update as Update gets only called before actually
    // all data is correct
    m_lateUpdate = true;
  }

  public void DestroyDebugView()
  {
    var lightLUT = GameObject.Find("LightLUT");
    lightLUT.DestroyUniveral();

    var iesLUT = GameObject.Find("IESLUT");
    iesLUT.DestroyUniveral();

    var cfoLUT = GameObject.Find("CFOLUT");
    cfoLUT.DestroyUniveral();
  }

  public void InitDebugView()
  {
    if (GameObject.Find("LightLUT") == null)
    {
      var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
      quad.name = "LightLUT";
      quad.transform.parent = Camera.main.transform;
      quad.transform.localEulerAngles = Vector3.zero;
      quad.transform.localScale = new Vector3(2, 0.1f, 1);
      quad.transform.localPosition = new Vector3(0, -1, 2);
      var mat = new Material(Shader.Find("Unlit/Texture"));
      mat.SetTexture("_MainTex", m_lightLUT);
      quad.GetComponent<Renderer>().material = mat;
    }
    if (GameObject.Find("IESLUT") == null)
    {
      var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
      quad.name = "IESLUT";
      quad.transform.parent = Camera.main.transform;
      quad.transform.localEulerAngles = Vector3.zero;
      quad.transform.localScale = new Vector3(2, 0.1f, 1);
      quad.transform.localPosition = new Vector3(0, -0.9f, 2);
      var mat = new Material(Shader.Find("Unlit/Texture"));
      mat.SetTexture("_MainTex", m_globalIES);
      quad.GetComponent<Renderer>().material = mat;
    }
    if (GameObject.Find("CFOLUT") == null)
    {
      var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
      quad.name = "CFOLUT";
      quad.transform.parent = Camera.main.transform;
      quad.transform.localEulerAngles = Vector3.zero;
      quad.transform.localScale = new Vector3(2, 0.1f, 1);
      quad.transform.localPosition = new Vector3(0, -0.8f, 2);

      m_matDebugCFOLUT = new Material(Shader.Find("Unlit/Texture"));
      m_matDebugCFOLUT.SetTexture("_MainTex", m_globalCFO);
      quad.GetComponent<Renderer>().material = m_matDebugCFOLUT;
    }
  }

  public enum UpdateMode
  {
    Update, FixedUpdate, LateUpdate
  }
}
