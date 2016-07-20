using UnityEngine;
using System.Collections;

public class drawLine : MonoBehaviour {
    public Transform A;
    public Transform B;
    public float width;
    LineRenderer lineRenderer;
    public Color color;
    // Use this for initialization
    void Start () {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
    }
	
	// Update is called once per frame
	void Update () {

        lineRenderer.useWorldSpace = true;
        lineRenderer.SetPosition(0, A.position);
        lineRenderer.SetPosition(1, B.position);
        lineRenderer.SetWidth(width, width);
        lineRenderer.SetColors(color, color);

    }
}
