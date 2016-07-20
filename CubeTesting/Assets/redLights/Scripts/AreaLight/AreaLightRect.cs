#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

public class AreaLightRect : AreaLight
{
  // Global Variables
  public float Width;
  public float Height;
  public float CosineExp;

  public int EnlightenX = 2;
  public int EnlightenY = 2;
  
  // enlighten proxies
  GameObject[] m_enlightenEmitters;

  public override void Start()
  {
    base.Start();
    Type = EAreaLightType.Rectangular;
    CosineExp = 1.0f;
  }

  public override void WriteLightCustomData(ref Texture2D lightLUT, int index, float packAdd, float packMult)
  {
    var dim = new Vector2((Width * 0.5f + packAdd) * packMult, (Height * 0.5f + packAdd) * packMult);
    var cosexp = (CosineExp + packAdd) * packMult;

    var rangeMult = Mathf.Max(Width, Height) * 0.5f;
    rangeMult = Mathf.Max(rangeMult, 1.0f);

    // TODO: pull into base class
    float falloffIdx = FalloffType == AreaLightFalloffType.Default ? 0 : LightIndex + 1;

    lightLUT.SetPixel(index + 5, 0, new Color(dim.x, dim.y, cosexp, falloffIdx / 255.0f));
    lightLUT.SetPixel(index + 6, 0, new Color(rangeMult, 0.0f, 0.0f, 0.0f));
  }

  public override void DoUpdate()
  {
    //FIXME: put this in a check update function?!
    if (EmissiveTexture && Type != EAreaLightType.RectangularTextured)
    {
      Type = EAreaLightType.RectangularTextured;
    }
    else if(EmissiveTexture == null && Type != EAreaLightType.Rectangular)
    {
      Type = EAreaLightType.Rectangular;
    }

    base.DoUpdate();

    var scale = transform.localScale;
    scale.x = Width;
    scale.y = Height;
    scale.z = 1;
    transform.localScale = scale;
  }

  public override void CreateEnlightenProxies()
  {
    var numPointsX = Mathf.Clamp(EnlightenX, 2, 10);
    var numPointsY = Mathf.Clamp(EnlightenY, 2, 10);

    m_enlightenEmitters = new GameObject[numPointsX*numPointsY];

    // create grid of point lights
    for (var x = 0; x < numPointsX; ++x)
    {
      for (var y = 0; y < numPointsY; ++y)
      {
        var idx = x + (y * numPointsX);

        m_enlightenEmitters[idx] = new GameObject { hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector };
        //m_enlightenEmitters[idx] = new GameObject();

        // this does not work for 1 light (1 light should always be centered)
        var stepX = (float)x / (float)Mathf.Max((numPointsX - 1), 1);
        var stepY = (float)y / (float)Mathf.Max((numPointsY - 1), 1);
        var xPos = Mathf.Lerp(-0.95f, 0.95f, stepX) * 0.5f;
        var yPos = Mathf.Lerp(-0.95f, 0.95f, stepY) * 0.5f;

        //
        m_enlightenEmitters[idx].transform.parent = m_enlighten.transform;
        m_enlightenEmitters[idx].transform.localPosition = new Vector3(xPos, yPos, 0f);
        m_enlightenEmitters[idx].transform.localScale = new Vector3(1, 1, 1);
        m_enlightenEmitters[idx].transform.localRotation = Quaternion.Euler(0, 0, 0);

        var enlightenProxy = m_enlightenEmitters[idx].AddComponent<Light>();
        enlightenProxy.cullingMask = 0;
        enlightenProxy.renderMode = LightRenderMode.ForceVertex;
        enlightenProxy.type = LightType.Spot;
        
        // testing
        //enlightenProxy.cullingMask = LayerMask.GetMask("Jalla2");
        //enlightenProxy.hideFlags = HideFlags.None;
        //enlightenProxy.renderMode = LightRenderMode.ForcePixel;

      }
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
    var numPointsX = Mathf.Clamp(EnlightenX, 2, 10);
    var numPointsY = Mathf.Clamp(EnlightenY, 2, 10);

    if((numPointsX*numPointsY) != m_enlightenEmitters.Length)
    {
      SetupEnlighten(true);
      return;
    }

    // update light proxies
    var unityLight = GetComponent<Light>();

    //LH: i think this gives the best approximination

    // energy difference to size
    var gridSpace = (Width / numPointsX) + (Height / numPointsY); 

    for (int i = 0; i < m_enlightenEmitters.Length; ++i)
    {
      var enlightenProxy = m_enlightenEmitters[i].GetComponent<Light>();

      //TODO: do we need to integrate intensity? or is range enough?

      // rect approximination
      float multiplier = 0.5f;
      if (i > 2 && i < (m_enlightenEmitters.Length - 1))
      {
        multiplier *= 0.85f;
      }

      enlightenProxy.color = unityLight.color;
      enlightenProxy.spotAngle = 150;
      enlightenProxy.range = Range;
      enlightenProxy.intensity = unityLight.intensity * multiplier * gridSpace * 0.25f;
      enlightenProxy.bounceIntensity = BounceIntensity;
    }
  }

  public override float SizeIntensity()
  {
    return Width * Height;
  }

  public void OnDrawGizmosSelected()
  {
#if UNITY_EDITOR
    var pos = transform.position;
    var up = transform.up;
    var right = transform.right;
    var fw = transform.forward;
    var height = transform.localScale.y;
    var length = transform.localScale.x;

    GizmoHelper.DrawLine(pos, pos + fw * Range);

    var p1 = pos + right * length * 0.5f;
    var p2 = p1 + right * Range;
    GizmoHelper.DrawLine(p1, p2);

    p1 = pos - right * length * 0.5f;
    p2 = p1 - right * Range;
    GizmoHelper.DrawLine(p1, p2);

    p1 = pos + up * height * 0.5f;
    p2 = p1 + up * Range;
    GizmoHelper.DrawLine(p1, p2);

    p1 = pos - up * height * 0.5f;
    p2 = p1 - up * Range;
    GizmoHelper.DrawLine(p1, p2);

    var offHor = right * Width / 2;
    var offVert = up * Height / 2;

    var upLeftTop = pos + offVert - offHor;
    var upRightTop = pos + offVert + offHor;
    var downLeftTop = pos - offVert - offHor;
    var downRightTop = pos - offVert + offHor;

    var verts = new Vector3[] { upLeftTop, upRightTop, downRightTop, downLeftTop };
    var colFace = new Color(0, 0, 0, 0);
    Handles.DrawSolidRectangleWithOutline(verts, colFace, Handles.color);

#endif
  }
}
