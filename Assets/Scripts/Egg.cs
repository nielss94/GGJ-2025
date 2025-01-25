using UnityEngine;
using System;

public class Egg : MonoBehaviour
{
    public event Action OnBreak;

    [SerializeField]
    private bool canRebirth = true;
    public bool CanRebirth { get { return canRebirth; } }

    private Rigidbody rb;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Break(Action onBreak = null)
    {
        // TODO: play break animation
        onBreak?.Invoke();

        OnBreak?.Invoke();
    }

    public void Launch(float bulletForce)
    {
        rb.AddForce(transform.forward * bulletForce, ForceMode.Impulse);
    }

    public void SetCanRebirth(bool v)
    {
        canRebirth = v;
    }
}
