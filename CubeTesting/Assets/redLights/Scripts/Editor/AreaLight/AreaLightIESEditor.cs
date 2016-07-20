using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AreaLightIES))]
public class AreaLightIESEditor : AreaLightEditor
{
  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    var area = (AreaLightIES)target;
    Undo.RecordObject(area, "Inspector Change");

    //The file IESType could be deleted 
    //Search for the enum reference in the assemblies
    var searchResult = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        from type in assembly.GetTypes()
                        where type.Name == "IESType"
                        select type);

    //The IESType is not part of the assemblies
    if (!searchResult.Any())
    {
      GUI.color = Color.red;
      EditorGUILayout.LabelField("Reimport [redLights/Resources/IES]");
      return;
    }
    var enumType = searchResult.First();

    //The enum dosen't have any value
    var iesTypeValues = Enum.GetValues(enumType);
    if (iesTypeValues.Length < 1)
    {
      GUI.color = Color.red;
      EditorGUILayout.LabelField("No ies profile found in [redLights/Resources/IES]");
      return;
    }

    //apply the first value as default
    var currentType = Convert.ChangeType(iesTypeValues.GetValue(0), enumType);
    if (!String.IsNullOrEmpty(area.IESProfileName))
    {
      try
      {
        //try to look up the enum by the corresponding name in the area light
        currentType = Convert.ChangeType(Enum.Parse(enumType, area.IESProfileName), enumType);
      }
      catch (Exception)
      {
        //Nothing to do
      }
    }

    var newType = Convert.ChangeType(EditorGUILayout.EnumPopup("IES Profile", (Enum)currentType), enumType);
    area.IESProfileIndex = (int)newType;
    area.IESProfileName = newType.ToString();

    if (GUILayout.Button("Reimport profiles"))
    {
      var redLightDir = Directory.GetDirectories(Application.dataPath, "redLights", SearchOption.AllDirectories);
      if (!redLightDir.Any())
      {
        Debug.LogWarning("IES Folder not found [redLights/Resources/IES]");
      }
      else
      {
        //AssetDatabase.Refresh();
        var iesProfilePathAbs = redLightDir.First() + @"\Resources\IES";
        var iesProfilePaths = Directory.GetFiles(iesProfilePathAbs, "*.ies", SearchOption.AllDirectories);
        var projectRoot = Path.GetFullPath(Application.dataPath)
          .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
          .ToUpperInvariant();

        Debug.LogWarning(projectRoot);
        foreach (var iesProfilePath in iesProfilePaths)
        {
         var fullPath = Path.GetFullPath(iesProfilePath)
          .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
          .ToUpperInvariant();

          //to relative asset path
          var relativePath = fullPath.Replace(projectRoot, "");
          AssetDatabase.ImportAsset("Assets"+relativePath, ImportAssetOptions.ForceUpdate);  
        }
      }
    };
  }

  void OnSceneGUI()
  {
    var area = (AreaLightIES)target;
    //var light = area.GetComponent<Light>();

    var pos = area.transform.position;
    var up = area.transform.up;
    var right = area.transform.right;
    var fw = area.transform.forward;
    var radius = area.transform.localScale.x/2;

    //Handles.color = GizmoUtility.InvertedColor(light.color * light.intensity);
    //Handles.color = new Color(0.949f, 0.941f, 0.478f);
    Handles.DrawWireDisc(pos, fw, radius);
    Handles.DrawWireDisc(pos, right, radius);
    Handles.DrawWireDisc(pos, up, radius);

    //Handles.color = Color.grey;
    area.Range = Handles.RadiusHandle(area.transform.rotation, area.transform.position, area.Range);
  }
}