using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AreaLightSpherical))]
public class AreaLightSphericalEditor : AreaLightEditor
{
  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    var area = (AreaLightSpherical)target;

    Undo.RecordObject(area, "Inspector Change");
    area.Radius = EditorGUILayout.FloatField("Radius", area.Radius);
    area.Radius = Mathf.Max(0.000001f, area.Radius);
    area.Range = Mathf.Clamp(area.Range, area.Radius * 2, 10000.0f);

    if (GUI.changed)
    {
      EditorUtility.SetDirty(area);
    }
  }
    
  void OnSceneGUI(){

    EditorGUI.BeginChangeCheck(); 

    var area = (AreaLightSpherical)target;

    area.Radius = Mathf.Max(0.000001f, area.Radius);
    area.Radius = Handles.RadiusHandle(area.transform.rotation, area.transform.position, area.Radius, true);

    area.Range = Mathf.Max(area.Range, area.Radius * 2);
    area.Range = Handles.RadiusHandle(area.transform.rotation, area.transform.position, area.Range, true);

    if (EditorGUI.EndChangeCheck())
    {
      Undo.RecordObject(area, "Scale");
    }

    if (GUI.changed)
    {
      EditorUtility.SetDirty(target);
    }
  }
}