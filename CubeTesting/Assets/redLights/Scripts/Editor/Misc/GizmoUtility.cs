using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

[InitializeOnLoad]
public class GizmoUtility
{
  private static MethodInfo m_setGizmoEnabled;

  private static bool m_dirty = false;
  private static bool m_state = false;

  static GizmoUtility()
  {
    EditorApplication.update += () =>
    {
      if (m_dirty)
      {
        m_dirty = false;
        ToggleGizmos(m_state);
      }
    };
  }

   public static Color InvertedColor(Color color)
   {
       return new Color(1.0f-color.r, 1.0f-color.g, 1.0f-color.b);
   }

  public static void DisableLightGizmos()
  {
    m_state = false;
    m_dirty = true;
  }

  public static void EnableLightGizmos()
  {
    m_state = true;
    m_dirty = true;
  }

  private static void ToggleGizmos(bool gizmosOn)
  {
    var val = gizmosOn ? 1 : 0;
    var asm = Assembly.GetAssembly(typeof(Editor));
    var type = asm.GetType("UnityEditor.AnnotationUtility");
    if (type != null)
    {
      var getAnnotations = type.GetMethod("GetAnnotations", BindingFlags.Static | BindingFlags.NonPublic);
      m_setGizmoEnabled = type.GetMethod("SetGizmoEnabled", BindingFlags.Static | BindingFlags.NonPublic);

      var annotations = getAnnotations.Invoke(null, null);
        
      foreach (object annotation in (IEnumerable)annotations)
      {
        var annotationType = annotation.GetType();

        var classIdField = annotationType.GetField("classID", BindingFlags.Public | BindingFlags.Instance);
        var scriptClassField = annotationType.GetField("scriptClass", BindingFlags.Public | BindingFlags.Instance);
        var enabledField = annotationType.GetField("gizmoEnabled", BindingFlags.Public | BindingFlags.Instance);

        if (classIdField != null && scriptClassField != null && enabledField != null)
        {
          var classId = (int)classIdField.GetValue(annotation);
          var scriptClass = (string)scriptClassField.GetValue(annotation);

          if (classId == 108)
          {
            m_setGizmoEnabled.Invoke(null, new object[] { classId, scriptClass, val });
          }
        }
      }
    }
  }


}