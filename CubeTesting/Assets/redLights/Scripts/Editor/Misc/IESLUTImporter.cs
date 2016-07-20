using UnityEditor;
using UnityEngine;
using System.Collections;

public class IESLUTImporter : AssetPostprocessor
{
  public const string IESLUT_Filename = "IESLUT.png";

  public void OnPreprocessTexture()
  {

    if (assetPath.ToLower().EndsWith(IESLUT_Filename.ToLower()))
    {
      var textureImporter = (TextureImporter)assetImporter;
      textureImporter.npotScale = TextureImporterNPOTScale.None;
      textureImporter.textureFormat = TextureImporterFormat.RGB24;
      textureImporter.mipmapEnabled = false;
      textureImporter.filterMode = FilterMode.Bilinear;
      textureImporter.maxTextureSize = 512;
    }
  }
}
