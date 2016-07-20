using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GizmoHelper : MonoBehaviour 
{
  #if UNITY_EDITOR

  public static Material Mat = new Material(Shader.Find("redPlant/Handles"));

  public static void DrawWireDisc(Vector3 center, Vector3 normal, float radius)
  {
    var from = Vector3.Cross(normal, Vector3.up);

    if ((double)from.sqrMagnitude < 1.0 / 1000.0)
    {
      from = Vector3.Cross(normal, Vector3.right);
    }
    DrawWireArc(center, normal, from, 360f, radius);
  }

  public static void DrawWireArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius)
  {
    var dest = new Vector3[60];
    SetDiscSectionPoints(dest, 60, center, normal, from, angle, radius);
    DrawPolyLine(dest);
  }

  internal static void SetDiscSectionPoints(Vector3[] dest, int count, Vector3 center, Vector3 normal, Vector3 from, float angle, float radius)
  {
    from.Normalize();
    var quaternion = Quaternion.AngleAxis(angle / (float)(count - 1), normal);
    var vector3 = from * radius;
    for (var index = 0; index < count; ++index)
    {
      dest[index] = center + vector3;
      vector3 = quaternion * vector3;
    }
  }

  public static void DrawPolyLine(params Vector3[] points)
  {
    if (Event.current.type != EventType.Repaint)
      return;

    if (Mat == null)
    {
      //FIXME: warning?
      return;
    }

    var c = new Color(1f, 1f, 1f, 0.75f);

    Mat.SetColor("_Color", Handles.color);
    for (var i = 0; i < 2; i++)
    {
      Mat.SetPass(i);

      GL.PushMatrix();
      GL.MultMatrix(Handles.matrix);
      GL.Begin(1);
      GL.Color(c);
      for (var index = 1; index < points.Length; ++index)
      {
        GL.Vertex(points[index]);
        GL.Vertex(points[index - 1]);
      }
      GL.End();
      GL.PopMatrix();
    }
  }

  public static void DrawLine(Vector3 p1, Vector3 p2)
  {
    if (Event.current.type != EventType.Repaint)
      return;

    if (Mat == null)
    {
      //FIXME: warning?
      return;
    }

    var c = new Color(1f, 1f, 1f, 0.75f);

    Mat.SetColor("_Color", Handles.color);
    for (var i = 0; i < 2; i++)
    {
      Mat.SetPass(i);

      GL.PushMatrix();
      GL.MultMatrix(Handles.matrix);
      GL.Begin(1);
      GL.Color(c);
      GL.Vertex(p1);
      GL.Vertex(p2);
      GL.End();
      GL.PopMatrix();
    }
  }
  #endif
}
