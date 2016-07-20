using UnityEngine;
using System.Collections;

public class TestingScript : MonoBehaviour {

    public Transform Cube1;
    public Transform Cube2;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Debug.Log(Cube2.localPosition - Cube1.localPosition);
	}
}
