using UnityEngine;
using System.Collections;

public class MatSwitcher : MonoBehaviour 
{
  public int Current;
  public Material[] Materials;
  public MeshRenderer[] TargetRenderer;

  public void SwitchMaterial()
  {
    Current++;
    if (Current == Materials.Length)
    {
      Current = 0;
    }

    foreach (var renderer in TargetRenderer)
    {
      renderer.material = Materials[Current];
      DynamicGI.UpdateMaterials(renderer);
    }
  }
}
