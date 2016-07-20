using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour
{
  bool interAction = false;
  Vector3 startingPos;
  float deltaPos = 0.8f;

  public Transform MoveFrom;
  public Transform MoveTo;

  public Transform LookFrom;
  public Transform LookTo;

  public Camera Cam;
  public AnimationCurve Zoom;


  // Update is called once per frame
  void Update()
  {
    var pos = Vector3.zero;
    var down = false;
    var up = false;

    down = Input.GetMouseButtonDown(0);
    up = Input.GetMouseButtonUp(0);
    pos = Input.mousePosition;

    if (pos.x < 150)
    {
      return;
    }


    /*
    if (Input.touchCount > 0) 
    {
     down = Input.touches [0].phase == TouchPhase.Began;
     up = Input.touches [0].phase == TouchPhase.Ended;
     pos = Input.touches [0].position;
    } */

    if (down)
    {
      interAction = true;
      startingPos = pos;
    }


    if (up)
    {
      interAction = false;
    }

    if (interAction)
    {
      deltaPos += (startingPos.x - pos.x) / Screen.width;
      deltaPos = Mathf.Clamp01(deltaPos);
      startingPos = pos;
    }

    Cam.transform.position = MoveFrom.position + (MoveTo.position - MoveFrom.position) * deltaPos;
    Cam.transform.LookAt(LookFrom.position + (LookTo.position - LookFrom.position) * deltaPos);

    Cam.fieldOfView = 30f + 30f * Zoom.Evaluate(deltaPos);
  }
}
