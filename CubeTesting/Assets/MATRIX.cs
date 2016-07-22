using UnityEngine;
using System.Collections;

public class MATRIX : MonoBehaviour
{

    Matrix4x4 aMatrix;
    public Transform cubeAl;
    public Transform cubeBl;
    public Transform cubeAr;
    public Transform cubeBr;

    Matrix4x4 V = Matrix4x4.identity;

    void Start()
    {
        aMatrix = Matrix4x4.identity;
    }

    void Update()
    {
        if (Input.GetKeyDown("space")) //calibrate
        {
            for (int i = 0; i < 2; i++) // Execute Twice per Calibration for increased dynamic molecular vertex interpolation accuracy(science, bitch!).
            {
                Vector3 cubeAvector = cubeAl.position - cubeAr.position;
                float cubeLengthA = (cubeAl.position - cubeAr.position).magnitude;

                Vector3 cubeBvector = cubeBl.position - cubeBr.position;
                float cubeLengthB = (cubeBl.position - cubeBr.position).magnitude;

                Vector3 _scale = new Vector3(cubeLengthB / cubeLengthA,cubeLengthB / cubeLengthA, cubeLengthB / cubeLengthA);
                Vector3 _position = cubeBl.position - cubeAl.position;
                Quaternion _rotation = Quaternion.FromToRotation(cubeAvector, cubeBvector);

                V = Matrix4x4.TRS(_position, _rotation, _scale);   // https://docs.unity3d.com/ScriptReference/Matrix4x4.TRS.html

                cubeAl.position = V.MultiplyPoint3x4(cubeAl.position);
                cubeAr.position = V.MultiplyPoint3x4(cubeAr.position);
            }

        }
        else
        {
            cubeAl.position = V.MultiplyPoint3x4(cubeAl.position);
            cubeAr.position = V.MultiplyPoint3x4(cubeAr.position);
        }

    }
}

    
