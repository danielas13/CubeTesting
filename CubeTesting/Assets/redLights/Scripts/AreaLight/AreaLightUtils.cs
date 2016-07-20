using UnityEngine;
using System.Collections;

public static class AreaLightUtils  {

  public static void DestroyUniveral(this Object target)
  {
    if (Application.isPlaying)
    {
      GameObject.Destroy(target);
    }
    else
    {
      GameObject.DestroyImmediate(target);
    }
  }



}
