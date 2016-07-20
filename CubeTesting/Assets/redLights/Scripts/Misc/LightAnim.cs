using UnityEngine;
using System.Collections;

public class LightAnim : MonoBehaviour
{
  public static bool AnimColor;

  public float Speed = 0.05f;
  public bool RandomSpeed;

  private float m_angle = 0.0f;
  public float m_range = 2.2f;

  private float m_yStart;
  private float m_xStart;
  private float m_giStart;

  public Vector3 AnimAxis;

  private Light m_light;
  private AreaLight m_arealight;

	// Use this for initialization
	void Start()
	{
	  m_yStart = transform.position.y;
    m_xStart = transform.position.x;
    if(RandomSpeed)
    {
      Speed = Random.value * 0.05f;
    }
    

	  m_light = GetComponent<Light>();
    m_arealight = GetComponent<AreaLight>();

	  m_giStart = m_arealight.BounceIntensity;
	}
	
	// Update is called once per frame
	void Update()
	{
    m_angle += Speed * Time.deltaTime;
    if (m_angle > 360)
	  {
      m_angle -= 360;
	  }

	  var pos = transform.position;
    pos.y = m_range * Mathf.Sin(m_angle * Mathf.Rad2Deg) + m_yStart;
    pos.x = m_range * Mathf.Sin(m_angle * Mathf.Rad2Deg * 2) + m_xStart;

	  pos.y *= AnimAxis.y;
    pos.x *= AnimAxis.x;

	  transform.position = pos;

	  if (AnimColor)
	  {
	    m_light.GetComponent<Colorizer>().enabled = true;
      m_light.GetComponent<Colorizer>().speed = Speed * 500.0f;
	  }
    else
    {
      m_light.GetComponent<Colorizer>().enabled = false;
      m_light.color = Color.white;
    }

    m_arealight.BounceIntensity = m_giStart * DemoRedlights.GIMult;
	}


}
