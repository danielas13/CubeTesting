using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class DemoRedlightsGI : MonoBehaviour
{
  public GameObject Planes;

  public GameObject Rects;
  public GameObject Spheres;
  public GameObject Disks;
  public GameObject Tubes;

  public GameObject RectSingle;
  public GameObject SphereSingle;
  public GameObject DiskSingle;
  public GameObject TubeSingle;

  public GameObject Label;

  private GameObject SingleLight;
  private GameObject MultiLight;

  private bool m_isColorAnimating;

  public static float GIMult = 1.0f;

  public Scrollbar Scroll;
  public Scrollbar GIScroll;
  public Scrollbar DiffuseScroll;

  void Start()
  {
    SingleLight = TubeSingle;
    MultiLight = Tubes;

    TogglePlanes();
    ToggleRects();
    Single();

    Scroll.onValueChanged.AddListener((s) =>
    {
      AreaLightManager.Instance.SpecLightMultiplier = s;
    });

    GIScroll.onValueChanged.AddListener((s) =>
    {
      DynamicGI.indirectScale = s;
    });

    DiffuseScroll.onValueChanged.AddListener((s) =>
    {
      AreaLightManager.Instance.DiffuseLightMultiplier = s;
    });
  }

  public void Forward()
  {
    Camera.main.renderingPath = RenderingPath.Forward;
  }

  public void Deffered()
  {
    Camera.main.renderingPath = RenderingPath.DeferredShading;
  }

  public void ToggleSpheres()
  {
    SingleLight.SetActive(false);
    MultiLight.SetActive(false);

    Spheres.SetActive(true);
    Rects.SetActive(false);
    Tubes.SetActive(false);
    Disks.SetActive(false);

    SingleLight = SphereSingle;
    MultiLight = Spheres;

    SingleLight.SetActive(true);
    MultiLight.SetActive(false);
  }

  public void ToggleTubes()
  {
    SingleLight.SetActive(false);
    MultiLight.SetActive(false);

    Spheres.SetActive(false);
    Rects.SetActive(false);
    Tubes.SetActive(true);
    Disks.SetActive(false);

    SingleLight = TubeSingle;
    MultiLight = Tubes;

    SingleLight.SetActive(true);
    MultiLight.SetActive(false);
  }

  public void ToggleRects()
  {
    SingleLight.SetActive(false);
    MultiLight.SetActive(false);

    Spheres.SetActive(false);
    Rects.SetActive(true);
    Tubes.SetActive(false);
    Disks.SetActive(false);

    SingleLight = RectSingle;
    MultiLight = Rects;

    SingleLight.SetActive(true);
    MultiLight.SetActive(false);
  }

  public void ToggleDisks()
  {
    SingleLight.SetActive(false);
    MultiLight.SetActive(false);

    Spheres.SetActive(false);
    Rects.SetActive(false);
    Tubes.SetActive(false);
    Disks.SetActive(true);

    SingleLight = DiskSingle;
    MultiLight = Disks;

    SingleLight.SetActive(true);
    MultiLight.SetActive(false);
  }

  public void AnimColor()
  {
    m_isColorAnimating = !m_isColorAnimating;
    LightAnim.AnimColor = m_isColorAnimating;
    Colorizer.Animate = m_isColorAnimating;
  }

  public void TogglePlanes()
  {
    Planes.SetActive(true);
    //Label.SetActive(false);
    //Studio.SetActive(false);    
  }

  public void ToggleStudio()
  {
    Planes.SetActive(false);
    //Label.SetActive(true);
    //Studio.SetActive(true);    
  }

  public void Multi()
  {
    SingleLight.SetActive(false);
    MultiLight.SetActive(true);
  }

  public void Single()
  {
    SingleLight.SetActive(true);
    MultiLight.SetActive(false);
  }
}
