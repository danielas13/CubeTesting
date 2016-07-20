using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AreaLightDisk))]
public class AreaLightDiskEditor : AreaLightEditor
{

  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    var area = (AreaLightDisk)target;
   
    Undo.RecordObject(area, "Inspector Change");
    area.Radius = EditorGUILayout.FloatField("Radius", area.Radius);
    area.Radius = Mathf.Max(0, area.Radius);
    area.Angle = EditorGUILayout.FloatField("Angle", area.Angle);
    area.Angle = Mathf.Clamp(area.Angle, 0.5f, 179.0f);

    area.AngleFalloff = EditorGUILayout.FloatField("AngleFalloff", area.AngleFalloff);
    area.AngleFalloff = Mathf.Clamp(area.AngleFalloff, 1.0f, 10.0f);

    if (GUI.changed)
      EditorUtility.SetDirty(area);
  }

  void OnSceneGUI(){

    EditorGUI.BeginChangeCheck(); 

    var area = (AreaLightDisk)target;
    var rot = area.transform.localEulerAngles;

    area.transform.Rotate(Vector3.forward, 45);

    var height = area.Range;
    var pos = area.transform.position;
    var up = area.transform.up;
    var right = area.transform.right;
    var fw = area.transform.forward;
    var radius = area.transform.localScale.x;
    var radiusBottom = Mathf.Tan(Mathf.Deg2Rad * area.Angle / 2) * height;
    area.RadiusBottom = radiusBottom;

    var northTop = pos + up * radius;
    var eastTop = pos + right * radius;
    var southTop = pos - up * radius;
    var westTop = pos - right * radius;

    var northBottom = northTop + up * radiusBottom + fw * height;
    var eastBottom = eastTop + right * radiusBottom + fw * height;
    var southBottom = southTop - up * radiusBottom + fw * height;
    var westBottom = westTop - right * radiusBottom + fw * height;

    Handles.color = new Color(0.949f, 0.941f, 0.478f);
    Handles.DrawCapFunction dot = Handles.DotCap;
   
    northTop = Handles.Slider(northTop, up, HandleUtility.GetHandleSize(northTop) / 40, dot, 1);
    eastTop = Handles.Slider(eastTop, right, HandleUtility.GetHandleSize(eastTop) / 40, dot, 1);
    southTop = Handles.Slider(southTop, up, HandleUtility.GetHandleSize(southTop) / 40, dot, 1);
    westTop = Handles.Slider(westTop, right, HandleUtility.GetHandleSize(westTop) / 40, dot, 1);

    northBottom = Handles.Slider(northBottom, up, HandleUtility.GetHandleSize(northBottom) / 40, dot, 1);
    eastBottom = Handles.Slider(eastBottom, right, HandleUtility.GetHandleSize(eastBottom) / 40, dot, 1);
    southBottom = Handles.Slider(southBottom, up, HandleUtility.GetHandleSize(southBottom) / 40, dot, 1);
    westBottom = Handles.Slider(westBottom, right, HandleUtility.GetHandleSize(westBottom) / 40, dot, 1);

    area.NorthBottom = northBottom;
    area.NorthTop = northTop;

    area.EastBottom = eastBottom;
    area.EastTop = eastTop;

    area.WestBottom = westBottom;
    area.WestTop = westTop;

    area.SouthBottom = southBottom;
    area.SouthTop = southTop;

    var diffRadius = (((northTop - pos).magnitude + (eastTop - pos).magnitude + (southTop - pos).magnitude + (westTop - pos).magnitude) / 4) - radius;
    var diffAngle = (Mathf.Rad2Deg * (Mathf.Acos(height / (northBottom - northTop).magnitude) + Mathf.Acos(height / (eastBottom - eastTop).magnitude) + Mathf.Acos(height / (westBottom - westTop).magnitude) + Mathf.Acos(height / (southBottom - southTop).magnitude)) / 4)*2 - area.Angle;

    if (EditorGUI.EndChangeCheck())
    {
        Undo.RecordObject(area, "Scale");
        area.Radius += diffRadius * 2;
        area.Angle += diffAngle * 2;
    }

    area.transform.localEulerAngles = rot;
      
    if (GUI.changed)
      EditorUtility.SetDirty(target);
  }
}