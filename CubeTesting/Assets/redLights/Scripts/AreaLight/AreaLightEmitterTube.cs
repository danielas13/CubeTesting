#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class AreaLightEmitterTube : AreaLightEmitter
{
  private GameObject m_top;
  private GameObject m_bottom;
  private GameObject m_tube;

  public override void CreateEmitter(Color color)
  {
    var tmpScale = transform.localScale;
    var tmpPos = transform.position;
    var tmpEuler = transform.eulerAngles;
    transform.localScale = Vector3.one;
    transform.position = Vector3.zero;
    transform.eulerAngles = Vector3.zero;

    if (transform.FindChild("Emitter") != null)
    {
      transform.FindChild("Emitter").gameObject.DestroyUniveral();
    }

    if (transform.FindChild("Top") != null)
    {
      transform.FindChild("Top").gameObject.DestroyUniveral();
    }

    if (transform.FindChild("Bottom") != null)
    {
      transform.FindChild("Bottom").gameObject.DestroyUniveral();
    }

    var rot = Vector3.zero;
    var scale = new Vector3(2, 2, 1);

    switch (m_areaLight.Type)
    {
      case EAreaLightType.Tube:
      case EAreaLightType.Line:
        var prefab = Resources.Load("Models/tube") as GameObject;
        m_emitter = m_tube = Instantiate(prefab) as GameObject;
        rot = new Vector3(0, 90, 90);
        break;
    }

    if (m_tube != null)
    {
      m_tube.name = "Emitter";
      m_tube.hideFlags = HideFlags.HideInHierarchy;
      m_tube.transform.parent = transform;
      m_tube.transform.localPosition = Vector3.zero;
      m_tube.transform.localEulerAngles = rot;
      m_tube.transform.localScale = scale;

      CreateMaterial();
      m_mat.SetColor("_Color", color);

      m_tube.GetComponent<Renderer>().sharedMaterial = m_mat;

      m_tube.GetComponent<Collider>().DestroyUniveral();

      var prefab = Resources.Load("Models/cap") as GameObject;

      m_top = Instantiate(prefab) as GameObject;
      m_top.hideFlags = HideFlags.HideInHierarchy;
      m_bottom = Instantiate(prefab) as GameObject;
      m_bottom.hideFlags = HideFlags.HideInHierarchy;

      m_top.name = "Top";
      m_bottom.name = "Bottom";

      m_top.transform.parent = transform;
      m_bottom.transform.parent = transform;

      m_top.transform.localPosition = new Vector3(1, 0, 0);
      m_bottom.transform.localPosition = new Vector3(-1, 0, 0);

      m_bottom.transform.eulerAngles = new Vector3(0, -90, 0);
      m_top.transform.eulerAngles = new Vector3(0, 90, 0);

      m_top.GetComponent<Renderer>().sharedMaterial = m_mat;
      m_bottom.GetComponent<Renderer>().sharedMaterial = m_mat;

      transform.localScale = tmpScale;
      transform.eulerAngles = tmpEuler;
      transform.position = tmpPos;
    }
  }

  public override void SelectParent()
  {
#if UNITY_EDITOR
    if (Selection.activeGameObject == m_bottom ||
        Selection.activeGameObject == m_top ||
        Selection.activeGameObject == m_tube)
    {
      Selection.activeGameObject = gameObject;
    }
#endif
  }

  public override void OnDestroy()
  {
    base.OnDestroy();
    m_top.DestroyUniveral();
    m_bottom.DestroyUniveral();
    m_tube.DestroyUniveral();
  }

  public override void Update()
  {
    base.Update();
    var scale = transform.localScale;
    var invScale = new Vector3(1, 1, scale.y / scale.x);
    m_bottom.transform.localScale = invScale;
    m_top.transform.localScale = invScale;
  }
}
