using UnityEngine;

public enum ELightUpdate : int
{
  None = 0x00,
  Local = 0x01,
  Global = 0x02,
  All = Local | Global
}

public struct AreaLightData
{
  public EAreaLightType Type;
  public float Width;
  public float Height;
  public float Radius;
  public float Range;
  public float Angle;

  public int IESIndex;

  public Vector3 Location;
  public Quaternion LocalRotation;
  public Vector3 LocalScale;

  // user emissive texture
  public Texture2D EmissiveTex;

  // internal cookie texture
  public Texture2D Cookie;

  public ELightUpdate NeedsUpdate(ref AreaLightData other)
  {
    var res = ELightUpdate.None;

    if (Type != other.Type || Width != other.Width || Height != other.Height || Radius != other.Radius ||
        Location != other.Location || LocalRotation != other.LocalRotation || LocalScale != other.LocalScale || Range != other.Range || Angle != other.Angle || IESIndex != other.IESIndex)
    {
      res |= ELightUpdate.Global;
    }

    if (EmissiveTex != other.EmissiveTex)
    {
      res |= ELightUpdate.Local;
    }

    return res;
  }

  public void UpdateValues(ELightUpdate update, ref AreaLightData other)
  {
    if ((update & ELightUpdate.Global) != 0)
    {
      Type = other.Type;
      Width = other.Width;
      Height = other.Height;
      Radius = other.Radius;
      Range = other.Range;
      Angle = other.Angle;
      Location = other.Location;
      LocalRotation = other.LocalRotation;
      LocalScale = other.LocalScale;
    }

    if((update & ELightUpdate.Local) != 0)
    {
      EmissiveTex = other.EmissiveTex;
    }
  }
}
