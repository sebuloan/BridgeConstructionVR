using UnityEngine;

public class Oscillate : MonoBehaviour
{
    [Header("Axis Selection")]
    public bool animateX = false;
    public bool animateY = false;
    public bool animateZ = false;

    [Header("Position Range")]
    public float startX = 0f;
    public float endX = 1f;
    public float startY = 0f;
    public float endY = 1f;
    public float startZ = 0f;
    public float endZ = 1f;

    [Header("Speed")]
    public float speed = 1f;

    private Vector3 _initialPosition;
    private float _timeCounter = 0f;

    void Start()
    {
        _initialPosition = transform.localPosition;
    }

    void Update()
    {
        _timeCounter += Time.deltaTime * speed;

        float xPos = _initialPosition.x;
        float yPos = _initialPosition.y;
        float zPos = _initialPosition.z;

        if (animateX)
        {
            xPos = Mathf.Lerp(startX, endX, Mathf.PingPong(_timeCounter, 1f));
        }

        if (animateY)
        {
            yPos = Mathf.Lerp(startY, endY, Mathf.PingPong(_timeCounter, 1f));
        }

        if (animateZ)
        {
            zPos = Mathf.Lerp(startZ, endZ, Mathf.PingPong(_timeCounter, 1f));
        }

        transform.localPosition = new Vector3(xPos, yPos, zPos);
    }
}