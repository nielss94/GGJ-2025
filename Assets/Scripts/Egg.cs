using UnityEngine;
using System;
using UnityEngine.PlayerLoop;

public class Egg : MonoBehaviour
{
    public event Action OnBreak;

    [SerializeField]
    private bool canRebirth = true;
    public bool CanRebirth { get { return canRebirth; } }

    private Rigidbody rb;

    [SerializeField]
    private float breakSpeedThreshold = 0.1f;
    private bool isBreaking = false;
    private float aliveTime = 0f;

    [SerializeField]
    private float minimumTimeBeforeSpeedCheck = 0.2f;

    private bool inRebirthFreeZone = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        aliveTime = 0f;
    }

    public void Break(Action onBreak = null)
    {
        isBreaking = true;
        // TODO: play break animation
        onBreak?.Invoke();

        OnBreak?.Invoke();
        isBreaking = false;
        Events.EggSpeed(0f);
    }

    public void Launch(float bulletForce)
    {
        rb.AddForce(transform.forward * bulletForce, ForceMode.Impulse);
    }

    void Update()
    {
        if (rb == null || UIManager.Instance == null) return;
        Events.EggSpeed(rb.linearVelocity.magnitude);

        aliveTime += Time.deltaTime;

        if (rb.linearVelocity.magnitude < breakSpeedThreshold && !isBreaking && aliveTime > minimumTimeBeforeSpeedCheck && inRebirthFreeZone)
        {
            isBreaking = true;
            Break();
        }
    }

    public void SetInRebirthFreeZone(bool v)
    {
        inRebirthFreeZone = v;
        canRebirth = !v;
    }
}