using UnityEngine;
using System.Collections;

public class MATRIX : MonoBehaviour {

    Matrix4x4 aMatrix;
    public Transform cubeAl;
    public Transform cubeBl;
    public Transform cubeAr;
    public Transform cubeBr;

    // Use this for initialization
    void Start () {
        aMatrix = Matrix4x4.identity;
    

    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKey("space"))
        {
            Vector3 cubeAvector = cubeAl.position - cubeAr.position;
            Vector3 cubeBvector = cubeBl.position - cubeBr.position;
            float cubeLengthB = (cubeBl.position - cubeBr.position).magnitude;
            float cubeLengthA = (cubeAl.position - cubeAr.position).magnitude;

            Matrix4x4 T = _getTranslationMatrix(cubeBl.position - cubeAl.position);
            Matrix4x4 R = _getRotationMatrix(cubeAvector, cubeBvector);
            Matrix4x4 S = _getScalingMatrix(
                new Vector3(
                    cubeLengthB / cubeLengthA,
                    cubeLengthB / cubeLengthA,
                    cubeLengthB / cubeLengthA
                ));

            aMatrix =  S * R * T;
            //aMatrix = R;

            Debug.Log("Before: " + Vector3.Angle(cubeAvector, cubeBvector));
            cubeAl.position = aMatrix.MultiplyPoint3x4(cubeAl.position); // is this correct?
            cubeAr.position = aMatrix.MultiplyPoint3x4(cubeAr.position);
             cubeAvector = cubeAl.position - cubeAr.position;
             cubeBvector = cubeBl.position - cubeBr.position;
            Debug.Log("After: " + Vector3.Angle(cubeAvector, cubeBvector));
        }

    }

    //http://www.euclideanspace.com/maths/geometry/affine/matrix4x4/index.htm
    Matrix4x4 _getTranslationMatrix(Vector3 vTranslation)
    {
        Matrix4x4 T = Matrix4x4.identity;
        T.SetColumn(3, vTranslation);
        T.m33 = 1;
        return T;
    }

    //http://www.opengl-tutorial.org/beginners-tutorials/tutorial-3-matrices/
    Matrix4x4 _getScalingMatrix(Vector3 vScale)
    {
        Matrix4x4 S = Matrix4x4.identity;
        S.m00 = vScale.x;
        S.m11 = vScale.y;
        S.m22 = vScale.z;
        S.m33 = 1;
        return S;
    }

    //http://math.stackexchange.com/questions/180418/calculate-rotation-matrix-to-align-vector-a-to-vector-b-in-3d
    Matrix4x4 _getRotationMatrix(Vector3 Vive, Vector3 Kinect) // rotate Kinect onto Vive
    {
        Vive = Vive.normalized;         
        Kinect = Kinect.normalized;

        //Vector3 v = Vector3.Cross(Vive, Kinect);
        Vector3 v = Vector3.Cross(Kinect, Vive);

        //  http://answers.unity3d.com/questions/24983/how-to-calculate-the-angle-between-two-vectors.html
        // Determine whether angle is positive or negative, needed since Vector3.Angle is always 0-180°
        Vector3 referenceForward = Vive;
            Vector3 referenceRight = Vector3.Cross(Vector3.up, referenceForward);
            Vector3 newDirection = Kinect;
            float _angle = Vector3.Angle(newDirection, referenceForward);       // note: degrees
            float sign = Mathf.Sign(Vector3.Dot(newDirection, referenceRight));
            float Angle = sign * _angle;

        Angle = Mathf.Deg2Rad * Angle;                                                              
        float s = v.magnitude * (Mathf.Sin(Angle));
        //float c = (Vector3.Dot(Vive, Kinect)) * (Mathf.Cos(Angle));
        float c = (Vector3.Dot(Kinect, Vive)) * (Mathf.Cos(Angle));

        Matrix4x4 vMatrix = Matrix4x4.zero;
        vMatrix[0, 0] = 0;    vMatrix[0, 1] = -v.z;  vMatrix[0, 2] = v.y;   vMatrix[0, 3] = 0;
        vMatrix[1, 0] = v.z;  vMatrix[1, 1] = 0;     vMatrix[1, 2] = -v.x;  vMatrix[1, 3] = 0;
        vMatrix[2, 0] = -v.y; vMatrix[2, 1] = v.x;   vMatrix[2, 2] = 0;     vMatrix[2, 3] = 0;
        vMatrix[3, 0] = 0;    vMatrix[3, 1] = 0;     vMatrix[3, 2] = 0;     vMatrix[3, 3] = 0;

        Matrix4x4 R_lhs = add(Matrix4x4.identity, vMatrix);                     // R_lhs = I + [vMatrix]
        Matrix4x4 R_rhs = scale(vMatrix * vMatrix, ((1 - c) / (s * s)));        // R_rhs = [vMatrix]^2 * (1-c / s^2) 
        Matrix4x4 R = add(R_lhs, R_rhs);   

        if (s == 0) //if cross product is 0 then  vectors are aligned (then S is zero and division above will return NAN)
        {
            return Matrix4x4.identity;
        }

        // The following R.m[xx] cant be right, since Vx squared should always affect the identity row
        // However this change seems to make the rotation work (over a few iterations), which is very unexpected
           R.m00 = 1; R.m11 = 1; R.m22 = 1; R.m33 = 1;

        return R;
    }

    /* Matrix multiplication by a float */
    Matrix4x4 scale(Matrix4x4 a, float s)
    {
        Matrix4x4 vMatrix = new Matrix4x4();
        vMatrix = Matrix4x4.zero;

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                vMatrix[i, j] = a[i, j] * s;
            }
        }
        return vMatrix;
    }

    /* Matrix Addition */
    Matrix4x4 add(Matrix4x4 a, Matrix4x4 b)
    {
        Matrix4x4 vMatrix = new Matrix4x4();
        for(int i = 0; i< 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                vMatrix[i, j] = a[i, j] + b[i, j];
            }
        }
        return vMatrix;
    }
}

    

