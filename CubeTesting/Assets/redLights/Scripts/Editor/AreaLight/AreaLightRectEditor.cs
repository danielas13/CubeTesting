using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AreaLightRect))]
public class AreaLightRectEditor : AreaLightEditor
{
  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    var area = (AreaLightRect)target;

    Undo.RecordObject(area, "Inspector Change");

    //FIXME: allow scene objects?!
    area.EmissiveTexture = EditorGUILayout.ObjectField("Emissive Texture", area.EmissiveTexture, typeof(Texture2D), false) as Texture2D;

    area.Width = Mathf.Max(0.000001f, area.Width);
    area.Width = EditorGUILayout.FloatField("Width", area.Width);

    area.Height = Mathf.Max(0.000001f, area.Height);    
    area.Height = EditorGUILayout.FloatField("Height", area.Height);
    
    area.EnlightenX = Mathf.Clamp(area.EnlightenX, 2, 10);
    area.EnlightenY = Mathf.Clamp(area.EnlightenY, 2, 10);
    area.EnlightenX = EditorGUILayout.IntField("Enlighten Proxies Width", area.EnlightenX);
    area.EnlightenY = EditorGUILayout.IntField("Enlighten Proxies Height", area.EnlightenY);

    if (GUI.changed)
    {
      EditorUtility.SetDirty(area);
    }
  }

  public void OnSceneGUI() 
  {
      EditorGUI.BeginChangeCheck();
    
      var area = (AreaLightRect)target;
      var up = area.transform.up;
      var right = area.transform.right;
      var pos = area.transform.position;

      var offHor = right * area.Width /2;
      var offVert = up * area.Height / 2;

      var centerUpTop = pos + offVert;
      var centerDownTop = pos - offVert;
      var centerLeftTop = pos + offHor;
      var centerRightTop = pos - offHor;

      Handles.color = new Color(0.949f, 0.941f, 0.478f);
      Handles.DrawCapFunction dot = Handles.DotCap;

      centerUpTop = Handles.Slider(centerUpTop, up, HandleUtility.GetHandleSize(centerUpTop) / 40, dot, 1);
      centerDownTop = Handles.Slider(centerDownTop, up, HandleUtility.GetHandleSize(centerUpTop) / 40, dot, 1);
      centerLeftTop = Handles.Slider(centerLeftTop, right, HandleUtility.GetHandleSize(centerUpTop) / 40, dot, 1);
      centerRightTop = Handles.Slider(centerRightTop, right, HandleUtility.GetHandleSize(centerUpTop) / 40, dot, 1);

      centerUpTop = Handles.Slider(centerUpTop, up, HandleUtility.GetHandleSize(centerUpTop) / 40, dot, 1);
      centerDownTop = Handles.Slider(centerDownTop, up, HandleUtility.GetHandleSize(centerUpTop) / 40, dot, 1);
      centerLeftTop = Handles.Slider(centerLeftTop, right, HandleUtility.GetHandleSize(centerUpTop) / 40, dot, 1);
      centerRightTop = Handles.Slider(centerRightTop, right, HandleUtility.GetHandleSize(centerUpTop) / 40 , dot, 1);
      centerUpTop = Handles.Slider(centerUpTop, up, HandleUtility.GetHandleSize(centerUpTop) / 40, dot, 1);
      centerDownTop = Handles.Slider(centerDownTop, up, HandleUtility.GetHandleSize(centerUpTop) / 40, dot, 1);
      centerLeftTop = Handles.Slider(centerLeftTop, right, HandleUtility.GetHandleSize(centerUpTop) / 40, dot, 1);
      centerRightTop = Handles.Slider(centerRightTop, right, HandleUtility.GetHandleSize(centerUpTop) / 40, dot, 1);

    if (EditorGUI.EndChangeCheck())
    {
        Undo.RecordObject(area, "Scale");
        area.Height = (centerUpTop - centerDownTop).magnitude;
        area.Width = (centerLeftTop - centerRightTop).magnitude;
    }

    if (GUI.changed)
    {
      EditorUtility.SetDirty(target);		      
    }
  }
}