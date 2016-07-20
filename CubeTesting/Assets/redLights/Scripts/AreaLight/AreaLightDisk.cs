#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class AreaLightDisk : AreaLight
{
  public float Radius;
  public float RadiusBottom;

  public float Angle;
  public float AngleFalloff;

  public Vector3 NorthTop;
  public Vector3 NorthBottom;

  public Vector3 SouthTop;
  public Vector3 SouthBottom;

  public Vector3 EastTop;
  public Vector3 EastBottom;

  public Vector3 WestTop;
  public Vector3 WestBottom;

  public override void Start()
  {
    base.Start();
    Type = EAreaLightType.Disk;
  }

  public override void WriteLightCustomData(ref Texture2D lightLUT, int index, float packAdd, float packMult)
  {
    var scale = transform.localScale;
    var width = Mathf.Abs(scale.x);
    var height = Mathf.Abs(scale.y);

    var dim = new Vector2((width*2 + packAdd) * packMult, (height*2 + packAdd) * packMult);
    var angle = (Angle * Mathf.Deg2Rad + packAdd) * packMult;

    var rangeMult = Radius * 0.5f;
    rangeMult = Mathf.Max(rangeMult, 1.0f);

    // TODO: pull into base class
    float falloffIdx = FalloffType == AreaLightFalloffType.Default ? 0 : LightIndex + 1;

    lightLUT.SetPixel(index + 5, 0, new Color(dim.x, dim.y, angle, falloffIdx / 255.0f));
    lightLUT.SetPixel(index + 6, 0, new Color(rangeMult, 0.0f, 0.0f, 1.0f));
  }

  public override void UpdateLight()
  {
    m_runtime.Angle = Angle;

    base.UpdateLight();
  }

  public override void DoUpdate()
  {
    base.DoUpdate();

    var scale = transform.localScale;
    scale.x = Radius;
    scale.y = Radius;
    scale.z = 1;
    transform.localScale = scale;

    Shader.SetGlobalFloat("_Angle", Angle);
    Shader.SetGlobalFloat("_AngleFalloff", AngleFalloff);
  }


  public override void CreateEnlightenProxies()
  {
    var enlightenProxy = m_enlighten.AddComponent<Light>();

    enlightenProxy.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
    enlightenProxy.cullingMask = 0;

    enlightenProxy.renderMode = LightRenderMode.ForceVertex;
    enlightenProxy.type = LightType.Spot;

    // testing
    //enlightenProxy.cullingMask = LayerMask.GetMask("Jalla2");
    //enlightenProxy.hideFlags = HideFlags.None;
    //enlightenProxy.renderMode = LightRenderMode.ForcePixel;
  }

  public override void UpdateEnlightenProxies()
  {
    var unityLight = GetComponent<Light>();
    var enlightenProxy = m_enlighten.GetComponent<Light>();

    enlightenProxy.bounceIntensity = BounceIntensity;
    enlightenProxy.color = unityLight.color;
    enlightenProxy.intensity = unityLight.intensity;
    enlightenProxy.range = Range;

    //TODO: check if we can get radius into this
    enlightenProxy.spotAngle = Angle * Mathf.Max(1.0f, Radius * 0.5f);
  }

  public override float SizeIntensity()
  {
    return Mathf.PI * Radius * 2.0f;
  }

  public void OnDrawGizmosSelected()
  {
#if UNITY_EDITOR
    GizmoHelper.DrawLine(NorthBottom, NorthTop);
    GizmoHelper.DrawLine(SouthBottom, SouthTop);
    GizmoHelper.DrawLine(EastBottom, EastTop);
    GizmoHelper.DrawLine(WestBottom, WestTop);

    var height = Range;
    var pos = transform.position;
    var fw = transform.forward;
    var radius = transform.localScale.x;

    GizmoHelper.DrawWireDisc(pos, fw, radius);
    GizmoHelper.DrawWireDisc(pos + fw * height, fw, radius + RadiusBottom);
#endif
  }
}

