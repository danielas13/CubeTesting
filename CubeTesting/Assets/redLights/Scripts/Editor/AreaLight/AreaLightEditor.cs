using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(AreaLight))]
public class AreaLightEditor : Editor
{
  private List<string> m_options = new List<string>();
  //private SortedDictionary<int, string> m_layerMaskCustomized;

  public override void OnInspectorGUI()
  {
    var area = (AreaLight)target;
    var light = area.GetComponent<Light>();

    // sometimes this are not set right 
    light.hideFlags = HideFlags.HideInInspector;
    if (area.HasEmitter && area.GetEmitter())
    {
      area.GetEmitter().hideFlags = HideFlags.HideInInspector;
    }

    Undo.RecordObject(light, "Inspector Change");
    // only change on edit
    var lightColor = area.Color;
    lightColor = EditorGUILayout.ColorField("Color", lightColor);
    if (lightColor != area.Color)
    {
      area.Color = lightColor;
    }

    var falloff = area.FalloffIndex;
    falloff = EditorGUILayout.IntField("FalloffIndex", falloff);
    if (falloff != area.FalloffIndex)
    {
      area.FalloffIndex = falloff;
    }

    // only change on edit
    var lightIntensity = area.Intensity;
    lightIntensity = EditorGUILayout.Slider("Intensity", lightIntensity, 0.0f, 8f);
    if (lightIntensity != area.Intensity)
    {
      area.Intensity = lightIntensity;
    }

    //Undo.RecordObject(area, "Inspector Change");
    area.Range = Mathf.Max(0.001f, area.Range);
    area.Range = EditorGUILayout.FloatField("Range", area.Range);

    var type = EditorGUILayout.EnumPopup("Falloff Type", area.FalloffType);
    area.FalloffType = (AreaLightFalloffType)Enum.Parse(typeof(AreaLightFalloffType), type.ToString());

    switch (area.FalloffType)
    {
      case AreaLightFalloffType.Default:
        break;

      case AreaLightFalloffType.Custom:

        EditorGUI.BeginChangeCheck();

        area.FalloffCurve = EditorGUILayout.CurveField("Falloff Curve", area.FalloffCurve);

        if (EditorGUI.EndChangeCheck())
        {
          area.UpdateFalloffLookup();          
        }

        break;
    }


    GUILayout.BeginHorizontal();

    var hasEmitter = area.HasEmitter;
    area.HasEmitter = EditorGUILayout.Toggle("Emittter", area.HasEmitter);

    if (hasEmitter != area.HasEmitter)
    {
      area.UpdateEmitter();
    }

    if (hasEmitter)
    {
      var transparentEmitter = area.IsTransparent;
      area.IsTransparent = EditorGUILayout.Toggle("Intensity to Alpha", area.IsTransparent);

      if (transparentEmitter != area.IsTransparent)
      {
        area.UpdateEmitterMaterial();
      }
    }

    GUILayout.EndHorizontal();

    if (hasEmitter && (area.Type == EAreaLightType.Disk || area.Type == EAreaLightType.Rectangular))
    {
      var doubleSided = area.IsDoubleSided;
      area.IsDoubleSided = EditorGUILayout.Toggle("Double Sided", area.IsDoubleSided);

      if (doubleSided != area.IsDoubleSided)
      {
        area.UpdateEmitterCulling();
      }
    }


    float preEditBounceIntensity = area.BounceIntensity;
    area.BounceIntensity = EditorGUILayout.Slider("Bounce Intensity", area.BounceIntensity, 0.0f, 8.0f);

    if (preEditBounceIntensity != area.BounceIntensity)
    {
      area.SetupEnlighten(false);
    }

    int mask = ReadFlags(light.cullingMask);
    int tmp_mask = EditorGUILayout.MaskField("Culling Mask", mask, m_options.ToArray());
    area.CullingMask = WriteFlags(tmp_mask);

    EditorUtility.SetDirty(area);
  }

  void OnEnable()
  {
    GizmoUtility.DisableLightGizmos();
    m_options = CloneLayerMask();
  }

  void OnDisable()
  {
    GizmoUtility.EnableLightGizmos();
  }

  // compact culling mask for mask field
  private int ReadFlags(int flags)
  {
    int mask = 0;
    int c = 0;
    for (var i = 0; i < 32; i++)
    {
      if (!(LayerMask.LayerToName(i) == null || LayerMask.LayerToName(i) == ""))
      {
        if ((flags & (1 << i)) != 0)
        {
          mask |= (1 << c);
        }
        c++;
      }

    }
    return mask;
  }

  // Writes compacted flags into culling mask
  private int WriteFlags(int flags)
  {
    if (flags == -1 || flags == 0)
    {
      return flags;
    }

    int mask = 0;
    int c = 1;
    int a = 0;
    for (var i = 0; i < 32; i++)
    {
      if (!(LayerMask.LayerToName(i) == null || LayerMask.LayerToName(i) == ""))
      {
        if ((flags & (1 << a)) != 0)
        {
          mask |= c;
        }
        a++;
      }
      c <<= 1;
    }
    return mask;
  }

  private List<string> CloneLayerMask()
  {
    var layers = new SortedDictionary<int, string>();
    for (var i = 0; i < 32; i++)
    {
      if (!layers.ContainsValue(LayerMask.LayerToName(i)))
      {
        layers.Add(1 << i, LayerMask.LayerToName(i));
      }
    }

    var list = new List<string>();
    foreach (var de in layers.Cast<DictionaryEntry>().Where(de => (string)de.Value != ""))
    {
      list.Add((string)de.Value);
    }

    return list;
  }

}