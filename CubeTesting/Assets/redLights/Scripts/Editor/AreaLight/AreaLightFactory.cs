using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class AreaLightFactory
{
  public const string RootMenu = "GameObject/Light/redLights/";
  public const string ToolsMenu = "GameObject/Light/redLights/Helper/";
  //public const string RootMenu = "redPlant/Lighting/";
  public const string PathRect = RootMenu + "Rect Light";
  public const string PathSphere = RootMenu + "Sphere Light";
  public const string PathTube = RootMenu + "Tube Light";
  public const string PathDisc = RootMenu + "Disc Light";
  public const string PathIES = RootMenu + "IES Light";
  public const string PathUpdateMaterials = ToolsMenu + "Update Materials";

  [MenuItem(PathRect, false, 0)]
  private static void createLightRect()
  {
    CreateLightInstance("redLight_Rect");
  }

  [MenuItem(PathSphere, false, 1)]
  private static void createLightSphere()
  {
    CreateLightInstance("redLight_Sphere");
  }

  [MenuItem(PathTube, false, 2)]
  private static void createLightTube()
  {
    CreateLightInstance("redLight_Tube");
  }

  [MenuItem(PathDisc, false, 3)]
  private static void createLightDisc()
  {
    CreateLightInstance("redLight_Disk");
  }

  [MenuItem(PathIES, false, 4)]
  private static void createLightIES()
  {
    CreateLightInstance("redLight_IES");
  }
  
  [MenuItem(PathUpdateMaterials, false, 1)]
  private static void updateMaterials()
  {
    // reapply all materials
    var materials = GetAssetsOfType<Material>(".mat");
    if (materials == null)
    {
      return;
    }

    var standardShader = AssetDatabase.LoadAssetAtPath("Assets/redLights/Resources/Shader/Standard.shader", typeof(Shader)) as Shader;
    var standardSpecularShader = AssetDatabase.LoadAssetAtPath("Assets/redLights/Resources/Shader/StandardSpecular.shader", typeof(Shader)) as Shader;

    if (standardShader && standardSpecularShader)
    {
      int matCounter = 0;
      foreach (var mat in materials)
      {
        if (mat.shader != null && mat.shader != standardShader &&
          mat.shader.name == "Standard")
        {
          matCounter++;
          mat.shader = standardShader;
          //Debug.Log("Updated Material " + mat.name + "(" + mat.shader.name + ")");
        }
        else if (mat.shader != null && mat.shader != standardSpecularShader &&
          mat.shader.name == "Standard (Specular setup)")
        {
          matCounter++;
          mat.shader = standardSpecularShader;
          //Debug.Log("Updated Material " + mat.name + "(" + mat.shader.name + ")");
        }
      }
      Debug.Log("Updated " + matCounter + " Materials");
    }
    else
    {
      Debug.LogError("Invalid Shaders, please reinstall redLights Package.");
    }
  }

  static string GetDirectory(string name)
  {
    var redLightDir = Directory.GetDirectories(Application.dataPath, "redLights", SearchOption.AllDirectories);
    var path = redLightDir[0] + name;
    return "Assets" + path.Replace(Application.dataPath, "");
  }

  static void CreateLightInstance(string name)
  {
    var path = GetDirectory("/Resources/Prefabs/");
    var tmp = AssetDatabase.LoadAssetAtPath(path + name + ".prefab", typeof(GameObject));
    var obj = Object.Instantiate(tmp) as GameObject;
    if (obj != null)
    {
      var pos = Vector3.zero;

      if (SceneView.lastActiveSceneView != null)
      {
        var rotation = SceneView.lastActiveSceneView.rotation;
        var dest = rotation * new Vector3(0, 0, 0.001f);
        pos = SceneView.lastActiveSceneView.pivot + dest;
        //pos = Vector3.one;
      }

      obj.transform.localPosition = pos;
      obj.name = name;
      Selection.activeGameObject = obj;
    }
  }

  //TAKEN FROM
  // http://answers.unity3d.com/questions/486545/getting-all-assets-of-the-specified-type.html
  public static T[] GetAssetsOfType<T>(string fileExtension) where T : UnityEngine.Object
  {
    var tempObjects = new List<T>();
    var directory = new System.IO.DirectoryInfo(Application.dataPath);
    var goFileInfo = directory.GetFiles("*" + fileExtension, System.IO.SearchOption.AllDirectories);

    var i = 0; var  goFileInfoLength = goFileInfo.Length;
    for (; i < goFileInfoLength; i++)
    {
      var tempGoFileInfo = goFileInfo[i];
      if (tempGoFileInfo == null)
      {
        continue;
      }

      var tempFilePath = tempGoFileInfo.FullName;
      tempFilePath = tempFilePath.Replace(@"\", "/").Replace(Application.dataPath, "Assets");
   
      var tempGO = AssetDatabase.LoadAssetAtPath(tempFilePath, typeof(T)) as T;
      if (tempGO != null && tempGO.GetType() == typeof(T))
      {
        tempObjects.Add(tempGO);
      }
    }

    return tempObjects.ToArray();
  }

}

