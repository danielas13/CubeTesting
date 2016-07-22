using UnityEngine;
using System.Collections;

public class orbit : MonoBehaviour {
    float rotationSpeed = 5.0f;
    public Transform _rotateAround;

    public bool orbitSystem = true;
    void Update()
    {
        if(orbitSystem)
        transform.RotateAround(_rotateAround.position, Vector3.up, 20 * Time.deltaTime);
    }

}
