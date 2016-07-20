using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class IESPostImporter : AssetPostprocessor
{
  static void OnPostprocessAllAssets(
    String[] importedAssets,
    String[] deletedAssets,
    String[] movedAssets,
    String[] movedFromAssetPaths)
  {
    //Combine all changes assets
    var changedAssetPaths = importedAssets.ToList();
    changedAssetPaths.AddRange(deletedAssets);


    var redLightDir = Directory.GetDirectories(Application.dataPath, "redLights", SearchOption.AllDirectories);
    if (redLightDir == null || !redLightDir.Any())
    {
      Debug.LogWarning("IES Folder not found [redLights/Resources/IES]");
      return;
    }

    //path to the unity project without the asset folder
    var pathProjectRoot = Application.dataPath.Remove(Application.dataPath.Length - 6);
    var pathAbs = redLightDir.First() + @"/Resources/IES";
    var pathGen = redLightDir.First() + @"/Scripts/Generated";

    var iesProfilePaths = Directory.GetFiles(pathAbs, "*.ies", SearchOption.AllDirectories);

    //var iesProfilePaths = AssetDatabase.FindAssets(@". t:TextAsset", new[] { path }).Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToList();
    if (!iesProfilePaths.Any())
    {
      Debug.LogWarning("No ies profiles found [redLights/Resources/IES]");
      Debug.LogWarning("Please read [redLights/Resources/IES/readme] for details on how to import ies sample profiles");
      WriteIESClass(pathGen, "");
      return;
    }

    //is any of the installed ies profiles reimported or detelted
    var isAnyChange = false;
    foreach (var iesProfilePath in iesProfilePaths)
    {
      foreach (var changedAssetPath in changedAssetPaths)
      {
        var changedAssetPathAbs = Path.Combine(pathProjectRoot, changedAssetPath);
        if (NormalizePath(iesProfilePath) == NormalizePath(changedAssetPathAbs))
        {
          
          isAnyChange = true;
          break;
        }
      }

      if (isAnyChange) break;
    }


    // No ies profile has changed -> over and out ;)
    if (!isAnyChange)
    {
      return;
    }

    //Some profile might have changed -> regenerate the lookup texture

    //Load all avalible textassets
    var iesEntities = new List<IESEntity>();
    foreach (var iesProfilePath in iesProfilePaths)
    {
      var iesEntity = new IESEntity()
      {
        Name = ToLowerSubset(Path.GetFileNameWithoutExtension(iesProfilePath),'_'),
        Raw = File.ReadAllText(iesProfilePath)
      };

      iesEntities.Add(iesEntity);
    }

    //create the combined lookup texture line by line
    //triple the heigth to get the advantage of the bilinear filtering in x and reduce the bilinear filterin in y
    var totalHeight = iesEntities.Count*3;
    var combinesIESTexture = new Texture2D(181, totalHeight, TextureFormat.RGB24, false);
    for (var i = 0; i < iesEntities.Count; i++)
    {
      var iesLine = IESParser.Parse(iesEntities[i].Raw);
      var pixelLine = iesLine.GetPixels();
      //set three lines at once
      combinesIESTexture.SetPixels(0, i * 3 + 0, 181, 1, pixelLine);
      combinesIESTexture.SetPixels(0, i * 3 + 1, 181, 1, pixelLine);
      combinesIESTexture.SetPixels(0, i * 3 + 2, 181, 1, pixelLine);
    }
    combinesIESTexture.Apply();
    File.WriteAllBytes(Path.Combine(pathAbs, IESLUTImporter.IESLUT_Filename), combinesIESTexture.EncodeToPNG());

    var enumLines = "";
    var enumId = 0;

    foreach (var iesEntity in iesEntities)
    {
      if (enumId > 0) enumLines += ",\n";
      enumLines += string.Format("{0} = {1}", iesEntity.Name.ToUpperInvariant(), enumId);
      enumId++;
    }

    WriteIESClass(pathGen, enumLines);

    AssetDatabase.Refresh(ImportAssetOptions.DontDownloadFromCacheServer);
  }

  private static void WriteIESClass(string path, string content)
  {
    const string enumFileTemplate = @"using System;
            public enum IESType{
              ###
            }
      ";

    var generatedEnum = enumFileTemplate.Replace("###", content);

    if (!Directory.Exists(path))
    {
      Directory.CreateDirectory(path);
    }

    File.WriteAllText(Path.Combine(path, "IESType.cs"), generatedEnum);
  }

  public static string ToLowerSubset(string source, char substitute)
  {
    var len = source.Length;
    var src = new StringBuilder(len);

    for (var i = 0; i < len; i++)
    {
      var singleChar = source.Substring(i,1);

      if (!Regex.IsMatch(singleChar, @"^[a-zA-Z0-9]+$"))
      {
        src.Append(substitute);
      }
      else
      {
        src.Append(singleChar);
      }
    }

    return src.ToString();
  }


  private static string NormalizePath(string path)
  {
    return Path.GetFullPath(path)
      .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
      .ToUpperInvariant();
  }

  internal class IESEntity
  {
    public string Name { get; set; }
    public string Raw { get; set; }

    public override string ToString()
    {
      return string.Format("Name: {0} Raw: {1}", Name, Raw);
    }
  }
}
