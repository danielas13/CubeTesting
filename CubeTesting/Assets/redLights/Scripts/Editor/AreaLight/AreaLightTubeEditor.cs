using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AreaLightTube))]
public class AreaLightTubeEditor : AreaLightEditor
{
  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    var area = (AreaLightTube)target;

    Undo.RecordObject(area, "Inspector Change");
    area.Radius = EditorGUILayout.FloatField("Radius", area.Radius);
    area.Length = EditorGUILayout.FloatField("Length", area.Length);
    area.Radius = Mathf.Max(0.000001f, area.Radius);
    area.Length = Mathf.Max(0.001f, area.Length);
    area.EnlightenProxies = Mathf.Clamp(area.EnlightenProxies, 2, 100);
    area.EnlightenProxies = EditorGUILayout.IntField("Enlighten Proxies", area.EnlightenProxies);

    if (GUI.changed)
    {
      EditorUtility.SetDirty(area);
    }
  }


  void OnSceneGUI()
  {
    EditorGUI.BeginChangeCheck();

    var area = (AreaLightTube)target;

    var pos = area.transform.position;
    var up = area.transform.up;
    var right = area.transform.right;
    var fw = area.transform.forward;
    var radius = area.transform.localScale.y;
    var length = area.transform.localScale.x;
    var range = area.Range;

    var top = pos + right * radius + right * length;
    var east = pos + fw * radius;
    var north = pos + up * radius;
    var south = pos - up * radius;
    var west = pos - fw * radius;
    var bottom = pos - right * radius - right * length;

    Handles.color = new Color(0.949f, 0.941f, 0.478f);
    Handles.DrawCapFunction dot = Handles.DotCap;

    top = Handles.Slider(top, right, HandleUtility.GetHandleSize(top) / 40, dot, 1);
    east = Handles.Slider(east, fw, HandleUtility.GetHandleSize(east) / 40, dot, 1);
    north = Handles.Slider(north, up, HandleUtility.GetHandleSize(north) / 40, dot, 1);
    south = Handles.Slider(south, up, HandleUtility.GetHandleSize(south) / 40, dot, 1);
    west = Handles.Slider(west, fw, HandleUtility.GetHandleSize(bottom) / 40, dot, 1);
    bottom = Handles.Slider(bottom, right, HandleUtility.GetHandleSize(bottom) / 40, dot, 1);

    var topRange = top + right * range;
    var eastRange = east + fw * range;
    var northRange = north + up * range;
    var southRange = south - up * range;
    var westRange = west - fw * range;
    var bottomRange = bottom - right * range;

    var diffRadius = (((east - pos).magnitude + (north - pos).magnitude + (south - pos).magnitude + (west - pos).magnitude) / 4) - radius;
    var diffRange = ((topRange - top).magnitude + (bottomRange - bottom).magnitude + (eastRange - east).magnitude + (westRange - west).magnitude + (northRange - north).magnitude + (southRange - south).magnitude) / 6 - range;

    if (EditorGUI.EndChangeCheck())
    {
      Undo.RecordObject(area, "Scale");
      area.Length = Mathf.Abs(((top - bottom).magnitude - area.Radius * 2) / 2);
      area.Radius += diffRadius;
      area.Range += diffRange;
    }

    if (GUI.changed)
      EditorUtility.SetDirty(target);
  }
}