using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Linq;

public class AlloyInstaller : MonoBehaviour
{

  [MenuItem("GameObject/Light/redLights/Helper/Setup Alloy Support", false, 0)]
  private static void SetupAlloy()
  {
    var dataPath = Application.dataPath;
    var filename = System.IO.Path.Combine(dataPath, "Alloy/Shaders/Config.cginc");

    if (System.IO.File.Exists(filename))
    {

      //FIXME: add encoding?!
      //var text = System.IO.File.ReadAllText(filename);

      var lines = System.IO.File.ReadAllLines(filename).ToList();

      var alreadyAdded = lines.Any((x) => { return x.Contains("ALLOY_SUPPORT_REDLIGHTS"); });

      if (!alreadyAdded)
      {

        lines.Insert(lines.Count - 2, @"");
        lines.Insert(lines.Count - 2, @"// redLight integration into Alloy Material System");
        lines.Insert(lines.Count - 2, @"#define ALLOY_SUPPORT_REDLIGHTS 1");
        lines.Insert(lines.Count - 2, "#include \"Assets/redLights/Resources/Shader/AlloySupport.cginc\"");
        lines.Insert(lines.Count - 2, @"");

        System.IO.File.WriteAllLines(filename, lines.ToArray());

        Debug.Log("Alloy support successfully installed, please reimport Alloy Shaders");
      }
      else
      {

        Debug.LogWarning("Alloy support alreaded installed.");
      }


    }
    else
    {
      Debug.LogError("Alloy not installed");
    }


  }
}
