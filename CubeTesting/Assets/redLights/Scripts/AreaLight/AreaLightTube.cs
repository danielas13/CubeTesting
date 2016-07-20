using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AreaLightTube : AreaLight
{
  // global variables
  public float Radius = 1;
  public float Length = 1;
  public int EnlightenProxies = 3;

  // enlighten proxies
  GameObject[] m_enlightenEmitters;

  public override void Start()
  {
    base.Start();
    Type = EAreaLightType.Tube;
  }

  public override void WriteLightCustomData(ref Texture2D lightLUT, int index, float packAdd, float packMult)
  {
    var radius = (Radius * Mathf.PI + packAdd) * packMult;
    var length = (Length + packAdd) * packMult;

    var rangeMult = Length * 0.5f;
    rangeMult = Mathf.Max(rangeMult, 1.0f);

    // TODO: pull into base class
    float falloffIdx = FalloffType == AreaLightFalloffType.Default ? 0 : LightIndex + 1;

    lightLUT.SetPixel(index + 5, 0, new Color(length, radius, 0.0f, falloffIdx / 255.0f));
    lightLUT.SetPixel(index + 6, 0, new Color(rangeMult, 0.0f, 0.0f, 1.0f));
  }

  public override void DoUpdate()
  {
    base.DoUpdate();

    var scale = transform.localScale;
    scale.x = Length;
    scale.y = Radius;
    scale.z = Radius;
    transform.localScale = scale;
  }

  protected override AreaLightEmitter GenerateEmitter()
  {
    if (m_emitter)
    {
      return m_emitter;
    }
    m_emitter = gameObject.GetComponent<AreaLightEmitter>();
    if (HasEmitter && m_emitter == null)
    {
      m_emitter = gameObject.AddComponent<AreaLightEmitterTube>();
    }
    return m_emitter;
  }

  public override void CreateEnlightenProxies()
  {
    var numPoints = Mathf.Clamp(EnlightenProxies, 2, 100);

    m_enlightenEmitters = new GameObject[numPoints];

    // create grid of point lights
    for (var i = 0; i < numPoints; ++i)
    {
      m_enlightenEmitters[i] = new GameObject { hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector };
      //m_enlightenEmitters[i] = new GameObject();

      // this does not work for 1 light (1 light should always be centered)
      var step = (float)i / (float)Mathf.Max((numPoints - 1), 1);
      var xPos = Mathf.Lerp(-1, 1, step);

      //
      m_enlightenEmitters[i].transform.parent = m_enlighten.transform;
      m_enlightenEmitters[i].transform.localPosition = new Vector3(xPos, 0f, 0f);
      m_enlightenEmitters[i].transform.localScale = new Vector3(1, 1, 1);
      m_enlightenEmitters[i].transform.localRotation = Quaternion.Euler(0, 0, 0);

      var enlightenProxy = m_enlightenEmitters[i].AddComponent<Light>();

      enlightenProxy.cullingMask = 0;
      enlightenProxy.renderMode = LightRenderMode.ForceVertex;
      enlightenProxy.type = LightType.Point;

      // testing
      //enlightenProxy.cullingMask = LayerMask.GetMask("Jalla2");
      //enlightenProxy.renderMode = LightRenderMode.ForcePixel;
      //enlightenProxy.hideFlags = HideFlags.None;
    }
  }

  public override void UpdateEnlightenProxies()
  {
    if (m_enlightenEmitters == null)
    {
      Debug.LogWarning("Emitters not created! Enlighten may not work.");
    }

    // check for length change (we need new points)
    // force recreating
    var numPoints = Mathf.Clamp(EnlightenProxies, 2, 100);
    if (numPoints != m_enlightenEmitters.Length)
    {
      SetupEnlighten(true);
      return;
    }

    // update light proxies
    var unityLight = GetComponent<Light>();

    float stepRange = (Length) / (float)m_enlightenEmitters.Length;

    for (int i = 0; i < m_enlightenEmitters.Length; ++i)
    {
      var enlightenProxy = m_enlightenEmitters[i].GetComponent<Light>();

      enlightenProxy.bounceIntensity = BounceIntensity;
      enlightenProxy.color = unityLight.color;

      // Size intensity with half PI (better approximiniation)
      enlightenProxy.intensity = unityLight.intensity * stepRange * 0.5f; // *(Mathf.PI * Radius * 0.5f);
      
      // line approximination
      var multiplier = 0.95f;
      if (i != 0 && i != m_enlightenEmitters.Length - 1)
      {
        multiplier = 1.0f;
      }
      //
      enlightenProxy.range = Range * multiplier + Radius;
    }
  }

  public override float SizeIntensity()
  {
    return Mathf.PI * Radius * Length * 2.0f;
  }

  public void OnDrawGizmosSelected()
  {
#if UNITY_EDITOR
    var pos = transform.position;
    var up = transform.up;
    var right = transform.right;
    var fw = transform.forward;
    var radius = transform.localScale.y;
    var length = transform.localScale.x;
    var range = Range;

    DrawTubeLight(pos, right, fw, up, radius, length, Handles.color);
    DrawTubeLight(pos, right, fw, up, radius + range, length, Handles.color);
#endif
  }

  private void DrawTubeLight(Vector3 pos, Vector3 right, Vector3 fw, Vector3 up, float radius, float length, Color color)
  {
#if UNITY_EDITOR
    var east = pos + fw * radius;
    var north = pos + up * radius;
    var south = pos - up * radius;
    var west = pos - fw * radius;

    Handles.color = color;

    GizmoHelper.DrawWireDisc(pos + right * length, right, radius);
    GizmoHelper.DrawWireDisc(pos - right * length, right, radius);

    GizmoHelper.DrawLine(north + right * length, north - right * length);
    GizmoHelper.DrawLine(south + right * length, south - right * length);
    GizmoHelper.DrawLine(east + right * length, east - right * length);
    GizmoHelper.DrawLine(west + right * length, west - right * length);

    GizmoHelper.DrawWireArc(pos + right * length, up, fw, 180, radius);
    GizmoHelper.DrawWireArc(pos - right * length, -up, fw, 180, radius);

    GizmoHelper.DrawWireArc(pos + right * length, -fw, up, 180, radius);
    GizmoHelper.DrawWireArc(pos - right * length, fw, up, 180, radius);
#endif
  }
}
