using UnityEngine;

public class AreaLightIES : AreaLight
{
  public float Radius = 0.25f;

  public int IESProfileIndex = 0;
  public string IESProfileName = "";

  public float IESMult = -0.5f;
  public float IESOffset = 0.5f;

  public override void Start()
  {
    base.Start();
    Type = EAreaLightType.IES;
  }

  public override void WriteLightCustomData(ref Texture2D lightLUT, int index, float packAdd, float packMult)
  {
    var scale = transform.localScale;
    var width = Mathf.Abs(scale.x);
    var height = Mathf.Abs(scale.y);

    var dim = new Vector2((width + packAdd) * packMult, (height + packAdd) * packMult);
    var profile = ((float)IESProfileIndex + packAdd) * packMult;

    // TODO: pull into base class
    float falloffIdx = FalloffType == AreaLightFalloffType.Default ? 0 : LightIndex + 1;

    lightLUT.SetPixel(index + 5, 0, new Color(dim.x, profile, 0.0f, falloffIdx / 255.0f));
    lightLUT.SetPixel(index + 6, 0, new Color(1.0f, 0.0f, 0.0f, 0.0f));
  }

  public override void UpdateLight()
  {
    m_runtime.IESIndex = IESProfileIndex;

    base.UpdateLight();
  }


  public override void DoUpdate()
  {
    base.DoUpdate();

    // force emitter radius
    Radius = 0.125f;

    var scale = transform.localScale;
    scale.x = Radius;
    scale.y = Radius;
    scale.z = Radius;
    transform.localScale = scale;

    IESMult = -0.5f;
    IESOffset = 0.5f;

    Shader.SetGlobalFloat("_IESMult", IESMult);
    Shader.SetGlobalFloat("_IESOffset", IESOffset);
  }

  //TODO: IES light use Spherical enlighten proxies
  public override void CreateEnlightenProxies()
  {
    var enlightenProxy = m_enlighten.AddComponent<Light>();

    // culling mask is always nothing
    enlightenProxy.cullingMask = 0;
    //TODO: check if this is needed...
    enlightenProxy.renderMode = LightRenderMode.ForceVertex;
    enlightenProxy.type = LightType.Point;
  }

  public override void UpdateEnlightenProxies()
  {
    var unityLight = GetComponent<Light>();
    var enlightenProxy = m_enlighten.GetComponent<Light>();

    enlightenProxy.bounceIntensity = BounceIntensity;
    enlightenProxy.color = unityLight.color;
    enlightenProxy.intensity = unityLight.intensity;
    enlightenProxy.range = Range;
  }
}
