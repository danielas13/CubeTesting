using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AreaLightManager))]
public class AreaLightManagerEditor : Editor
{
  private bool showAdvancedSettings = false;

  public override void OnInspectorGUI()
  {
    var lightManager = (AreaLightManager)target;

    showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Advanced Settings");

    Undo.RecordObject(lightManager, "Inspector Change");

    if (showAdvancedSettings)
    {
      //support for showing debug output on Main Camera
      lightManager.DebugView = EditorGUILayout.Toggle("Debug View", lightManager.DebugView);

      lightManager.SpecLightMultiplier = EditorGUILayout.FloatField("SpecLight Multiplier", lightManager.SpecLightMultiplier);
      lightManager.DiffuseLightMultiplier = EditorGUILayout.FloatField("DiffuseLight Multiplier", lightManager.DiffuseLightMultiplier);

      lightManager.GlobalIllumination = EditorGUILayout.Toggle("Global Illumination", lightManager.GlobalIllumination);

      bool fastMode = lightManager.FastMode;
      lightManager.FastMode = EditorGUILayout.Toggle("Fast Mode", lightManager.FastMode);

      bool early = lightManager.EarlyDiscard;
      lightManager.EarlyDiscard = EditorGUILayout.Toggle("Early Discard", lightManager.EarlyDiscard);

      bool specNorm = lightManager.SpecNormalization;
      lightManager.SpecNormalization = EditorGUILayout.Toggle("Specular Normalization", lightManager.SpecNormalization);

      if (fastMode != lightManager.FastMode || specNorm != lightManager.SpecNormalization || early != lightManager.EarlyDiscard)
      {
        WriteSettings(lightManager.FastMode, lightManager.SpecNormalization, lightManager.EarlyDiscard);
      }

      lightManager.UsedUpdateMode = (AreaLightManager.UpdateMode) EditorGUILayout.EnumPopup("Update Mode", lightManager.UsedUpdateMode);

      if (GUILayout.Button("Force Rebuild"))
      {
        WriteSettings(lightManager.FastMode, lightManager.SpecNormalization, lightManager.EarlyDiscard);
        lightManager.GatherLights();
      }
    }

    if (GUI.changed)
      EditorUtility.SetDirty(lightManager);
  }

  public void WriteSettings(bool fastMode, bool specNormalization, bool earlyDiscard)
  {
    string dataPath = Application.dataPath;
    string filename = System.IO.Path.Combine(dataPath, "redLights/Resources/Shader/redConfig.cginc");

    var sr = System.IO.File.CreateText(filename);
    sr.WriteLine("#ifndef _REDPLANT_CONFIG_HEADER_");
    sr.WriteLine("#define _REDPLANT_CONFIG_HEADER_");
    sr.WriteLine("//REDLIGHTS CONFIG FILE");
    // this forces unity to update file
    string date = "//WRITTEN " + System.DateTime.Now.ToLongTimeString();
    sr.WriteLine(date);

    // we are writing all values just to see all available options in config file
    if (fastMode)
    {
      sr.WriteLine("#define REDLIGHT_FAST");
    }
    else
    {
      sr.WriteLine("//#define REDLIGHT_FAST");
    }

    if (specNormalization)
    {
      sr.WriteLine("#define REDLIGHT_APPROX_D");
    }
    else
    {
      sr.WriteLine("//#define REDLIGHT_APPROX_D");
    }

    if (earlyDiscard)
    {
      sr.WriteLine("#define REDLIGHT_DO_EARLY_OUT");
    } 
    else
    {
      sr.WriteLine("//#define REDLIGHT_DO_EARLY_OUT");
    }

    sr.WriteLine("#endif");
    sr.Close();

    // force import
    //AssetDatabase.ImportAsset("redLights/Resources/Shader/redConfig.cginc", ImportAssetOptions.ForceUpdate);
    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
  }


}