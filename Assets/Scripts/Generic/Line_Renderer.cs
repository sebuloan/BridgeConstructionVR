using UnityEngine;

public class Line_Renderer : MonoBehaviour
{
    public Transform startPoint;  // Should be a child of first object
    public Transform endPoint;   // Should be a child of second object
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        if (startPoint != null && endPoint != null)
        {
            lineRenderer.SetPosition(0, startPoint.position);
            lineRenderer.SetPosition(1, endPoint.position);
        }
    }
}
