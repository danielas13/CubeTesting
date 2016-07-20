using System.Linq;
using UnityEngine;
using System.IO;

public class IESParser
{
  public static Texture2D AngleLookUp;

  public static Texture2D Parse(TextAsset file)
  {
    return Parse(file.text);
  }

  public static Texture2D Parse(string fileContent)
  {
    var stream = new StringReader(fileContent);
    string line = null;
    while (null != (line = stream.ReadLine()))
    {
      if (line.StartsWith("TILT="))
      {
        // 
        var data = stream.ReadLine().Replace("  ", " ").Split(' ');
        var numVertAngles = System.Convert.ToInt32(data[3]);

        data = stream.ReadLine().Split(' ');

        // vertical angles
        var count = 0;
        var vertAngles = new float[numVertAngles];
        while (count < numVertAngles)
        {
          data = stream.ReadLine().Split(' ');
          foreach (var angle in data.Where(angle => !string.IsNullOrEmpty(angle)))
          {
            vertAngles[count++] = System.Convert.ToSingle(angle);
          }
        }

        // skip horizontal for now
        data = stream.ReadLine().Split(' ');

        count = 0;
        var vertCandelas = new float[numVertAngles];
        var min = float.MaxValue;
        var max = float.MinValue;

        while (count < numVertAngles)
        {
          data = stream.ReadLine().Split(' ');
          foreach (var candela in data)
          {
            if (string.IsNullOrEmpty(candela))
            {
              continue;
            }

            vertCandelas[count] = System.Convert.ToSingle(candela);

            min = Mathf.Min(min, vertCandelas[count]);
            max = Mathf.Max(max, vertCandelas[count]);

            count++;
          }
        }

        // normalize the whole thing
        var norm = 1.0f / max;
        for (var x = 0; x < vertCandelas.Length; x++)
        {
          vertCandelas[x] = (vertCandelas[x] - min) * norm;
        }

        AngleLookUp = new Texture2D(numVertAngles, 1, TextureFormat.RGB24, false, false);
        for (var x = 0; x < vertCandelas.Length; x++)
        {
          for (var y = 0; y < 1; y++)
          {
            var candela = vertCandelas[x];
            AngleLookUp.SetPixel(x, y, new Color(candela, candela, candela, 1));
          }
        }
        AngleLookUp.wrapMode = TextureWrapMode.Clamp;
        AngleLookUp.filterMode = FilterMode.Bilinear;
        AngleLookUp.Apply(false);
      }
    }
    return AngleLookUp;
  }
}
