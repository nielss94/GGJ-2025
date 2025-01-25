using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField]
    private Transform pointA;
    [SerializeField]
    private Transform pointB;
    [SerializeField]
    private float speed = 1f;
    [SerializeField]
    private bool startActive = false;
    [SerializeField]
    private Rigidbody rb;

    private bool isActive;

    void Start()
    {
        isActive = startActive;
    }

    void FixedUpdate()
    {
        if (rb == null || !isActive) return;
        
        rb.MovePosition(Vector3.Lerp(pointA.position, pointB.position, Mathf.PingPong(Time.time * speed, 1f)));
    }

    public void Toggle()
    {
        isActive = !isActive;
    }
}
