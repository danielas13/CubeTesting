#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class LayerUtility {

  static LayerUtility()
  {
	}

  public static void InitLayer()
  {
    var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
    var it = tagManager.GetIterator();

    const bool showChildren = true;

    while (it.NextVisible(showChildren))
    {
      if (it.name == "layers") 
      {
        var redLightsIsSet = false;

        foreach (var o in it)
        {
          var p = (SerializedProperty)o;

          if (p.stringValue == "redlights")
          {
            redLightsIsSet = true;
            break;
          }
        }

        if (redLightsIsSet) 
        {
          break;
        }

        var usedLayers = 0;
        var layer = 0;

        foreach(var o in it)
        {
          var p = (SerializedProperty) o;
          
          if (p.stringValue == "" && layer > 7)
          {
            //Debug.Log(p.stringValue + " " + layer);
            p.stringValue = "redlights";
            break;
          }
          else
          {
            usedLayers++;
          }
          layer++;
        }

        if (usedLayers == 32)
        {
          throw new System.Exception("All your layers are in use. Please make sure that one layer is called 'redLights'");
        }
      }
    }
    tagManager.ApplyModifiedProperties();
  }
}

#endif