using UnityEngine;

public class AreaLightSpherical : AreaLight
{
  public float Radius;

  public override void Start()
  {
    base.Start();
    Type = EAreaLightType.Spherical;
  }

  public override void WriteLightCustomData(ref Texture2D lightLUT, int index, float packAdd, float packMult)
  {
    var scale = transform.localScale;
    var width = Mathf.Abs(scale.x);
    var height = Mathf.Abs(scale.y);

    var dim = new Vector2((width*2 + packAdd) * packMult, (height*2 + packAdd) * packMult);
    var radius = (Radius + packAdd) * packMult;

    var rangeMult = Radius * 0.5f;
    rangeMult = Mathf.Max(rangeMult, 1.0f);

    // TODO: pull into base class
    float falloffIdx = FalloffType == AreaLightFalloffType.Default ? 0 : LightIndex + 1;

    lightLUT.SetPixel(index + 5, 0, new Color(dim.x, dim.y, radius, falloffIdx / 255.0f));
    lightLUT.SetPixel(index + 6, 0, new Color(rangeMult, 0.0f, 0.0f, 1.0f));
  }

  public override void DoUpdate()
  {
    base.DoUpdate();

    var scale = transform.localScale;
    scale.x = Radius;
    scale.y = Radius;
    scale.z = Radius;
    transform.localScale = scale;
  }


  public override void CreateEnlightenProxies()
  {
    var enlightenProxy = m_enlighten.AddComponent<Light>();

    enlightenProxy.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
    enlightenProxy.cullingMask = 0;
    enlightenProxy.renderMode = LightRenderMode.ForceVertex;
    enlightenProxy.type = LightType.Point;

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
    enlightenProxy.intensity = unityLight.intensity * Radius;
    enlightenProxy.range = Range;
  }

  public override float SizeIntensity()
  {
    return Radius * 2.0f;
  }

  public void OnDrawGizmosSelected()
  {
#if UNITY_EDITOR
    var pos = transform.position;
    var up = transform.up;
    var right = transform.right;
    var fw = transform.forward;
    var radius = transform.localScale.x;

    GizmoHelper.DrawWireDisc(pos, fw, radius);
    GizmoHelper.DrawWireDisc(pos, right, radius);
    GizmoHelper.DrawWireDisc(pos, up, radius);

    GizmoHelper.DrawWireDisc(pos, fw, Range);
    GizmoHelper.DrawWireDisc(pos, right, Range);
    GizmoHelper.DrawWireDisc(pos, up, Range);
#endif
  }
}
