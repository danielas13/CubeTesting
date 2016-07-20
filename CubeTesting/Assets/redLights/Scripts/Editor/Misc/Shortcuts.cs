using UnityEngine;
using UnityEditor;
using System.Collections;

public class Shortcuts : MonoBehaviour
{
  [MenuItem("GameObject/Light/redLights/Helper/Toggle Forward  %1")]
  private static void ToggleForward()
  {
    Camera.main.renderingPath = RenderingPath.Forward;
    Debug.Log(Camera.main.renderingPath);
  }

  [MenuItem("GameObject/Light/redLights/Helper/Toggle Deferred  %2")]
  private static void ToggleShading()
  {
    Camera.main.renderingPath = RenderingPath.DeferredShading;
    Debug.Log(Camera.main.renderingPath);
  }
}
