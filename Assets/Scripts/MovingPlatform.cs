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
    private float timer = 0f;

    void Start()
    {
        isActive = startActive;
    }

    void FixedUpdate()
    {
        if (rb == null || !isActive) return;
        
        timer += Time.fixedDeltaTime * speed;
        rb.MovePosition(Vector3.Lerp(pointA.position, pointB.position, Mathf.PingPong(timer, 1f)));
    }

    public void Toggle()
    {
        isActive = !isActive;
    }
}
