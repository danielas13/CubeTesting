#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(AreaLight))]
[HideInInspector]
public class AreaLightEmitter : MonoBehaviour
{
  public bool IsTransparent = false;
  public bool IsDoubleSided = true;

  protected AreaLight m_areaLight;
  protected Material m_mat;
  protected Light m_light;

  protected GameObject m_emitter;

  // Update is called once per frame
  public virtual void Awake()
  {
    m_areaLight = GetComponent<AreaLight>();
    m_light = GetComponent<Light>();

    CreateEmitter(m_light.color);

#if UNITY_EDITOR
    EditorApplication.update += SelectParent;
#endif
  }

  public void OnEnable()
  {
    this.hideFlags = HideFlags.HideInInspector;
  }

  public virtual void SelectParent()
  {
#if UNITY_EDITOR
    if (Selection.activeGameObject == m_emitter)
    {
      Selection.activeGameObject = gameObject;
    }
#endif
  }

  public virtual void OnDestroy()
  {
#if UNITY_EDITOR
    EditorApplication.update -= SelectParent;
#endif
    if (m_emitter)
    {
      m_emitter.DestroyUniveral();
    }
  }

  public virtual void Update()
  {
    if (m_mat == null || m_emitter == null)
    {
      CreateEmitter(m_light.color);
    }

    if (m_mat != null && m_emitter != null)
    {
      var col = m_light.color * m_light.intensity;

      if (m_areaLight.EmissiveTexture)
      {
        m_mat.SetTexture("_Tex", m_areaLight.EmissiveTexture);
      }
      else
      {
        m_mat.SetTexture("_Tex", Texture2D.whiteTexture);
      }

      m_mat.SetColor("_Color", col);
      m_mat.SetFloat("_Alpha", IsTransparent ? Mathf.Clamp01(m_light.intensity) : 1.0f);
    }
  }

  public void UpdateCullMode()
  {
    if(m_emitter) {
      var shader = IsDoubleSided ? "redPlant/EmitterCullOff" : "redPlant/EmitterCullBack";
      m_emitter.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find(shader);    
    }
  }

  public virtual void CreateEmitter(Color color)
  {
    var tmpScale = transform.localScale;
    transform.localScale = Vector3.one;

    if (m_emitter)
    {
      m_emitter.DestroyUniveral();
    }

    if (transform.FindChild("Emitter") != null)
    {
      transform.FindChild("Emitter").gameObject.DestroyUniveral();
    }

    var rot = Vector3.zero;
    var scale = Vector3.one;

    switch (m_areaLight.Type)
    {
      case EAreaLightType.Rectangular:
      case EAreaLightType.RectangularTextured:
        m_emitter = GameObject.CreatePrimitive(PrimitiveType.Quad);
        rot = new Vector3(0, 180, 0);
        scale = new Vector3(1, 1, 1);
        break;
      case EAreaLightType.Spherical:
        m_emitter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        scale = new Vector3(2, 2, 2);
        break;
      case EAreaLightType.IES:
        m_emitter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        scale = new Vector3(1, 1, 1);
        break;
      case EAreaLightType.Disk:
        var prefab = Resources.Load("Models/disc") as GameObject;
        m_emitter = GameObject.Instantiate(prefab) as GameObject;
        scale = new Vector3(2, 2, 1);
        break;
    }
    
    if (m_emitter != null)
    {
      m_emitter.name = "Emitter";
      m_emitter.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
      m_emitter.transform.parent = transform;
      m_emitter.transform.localPosition = Vector3.zero;
      m_emitter.transform.localEulerAngles = rot;
      m_emitter.transform.localScale = scale;

      CreateMaterial();
      m_mat.SetColor("_Color", color);

      m_emitter.GetComponent<Renderer>().sharedMaterial = m_mat;
      m_emitter.GetComponent<Collider>().DestroyUniveral();
    }
    else
    {
      Debug.LogWarning("invalid emitter " + m_emitter + " with material " + m_mat);
    }

    transform.localScale = tmpScale;
  }

  protected void CreateMaterial()
  {
    if (m_mat == null)
    {
      var shader = IsDoubleSided ? "redPlant/EmitterCullOff" : "redPlant/EmitterCullBack";
      m_mat = new Material(Shader.Find(shader));

      if (m_emitter != null)
      {
        m_emitter.GetComponent<Renderer>().sharedMaterial = m_mat;
      }
    }
  }

}
