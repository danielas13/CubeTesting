using System;
using UnityEngine;
using System.Collections;

public class Colorizer : MonoBehaviour
{
  public static bool Animate;

  private Light m_areaLight;

  public int HueOffset;
  public float speed = 10f;


  // Use this for initialization
  void Start()
  {
    m_areaLight = GetComponent<AreaLight>().GetComponent<Light>();
  }

  // Update is called once per frame
  void Update()
  {
    if (!Animate)
    {
      m_areaLight.color = Color.white;
      return;
    }
      
    var newColor = ColorFromHSV(HueOffset + (Time.time * speed), 1d, 1d);
    m_areaLight.color = newColor;
  }

  public static Color ColorFromHSV(double hue, double saturation, double value)
  {
    int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
    double f = hue / 60 - Math.Floor(hue / 60);

    value = value * 255;
    int v = Convert.ToInt32(value);
    int p = Convert.ToInt32(value * (1 - saturation));
    int q = Convert.ToInt32(value * (1 - f * saturation));
    int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

    //Debug.Log(v);
    //Debug.Log(p);
    //Debug.Log(q);
    //Debug.Log(t);

    if (hi == 0)
      return new Color(v / 255f, t / 255f, p / 255f);
    else if (hi == 1)
      return new Color(q / 255f, v / 255f, p / 255f);
    else if (hi == 2)
      return new Color(p / 255f, v / 255f, t / 255f);
    else if (hi == 3)
      return new Color(p / 255f, q / 255f, v / 255f);
    else if (hi == 4)
      return new Color(t / 255f, p / 255f, v / 255f);
    else
      return new Color(v / 255f, p / 255f, q / 255f);
  }
}
