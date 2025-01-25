using UnityEngine;
using UnityEngine.Events;

public class TriggerInteraction : MonoBehaviour
{
    [SerializeField]
    private LayerMask layer;
    [SerializeField]
    private UnityEvent onTriggerEnter;
    [SerializeField]
    private UnityEvent onTriggerExit;


    private void OnTriggerEnter(Collider other)
    {
        if (layer == (layer | (1 << other.gameObject.layer)))
        {
            onTriggerEnter?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (layer == (layer | (1 << other.gameObject.layer)))
        {
            onTriggerExit?.Invoke();
        }
    }
}
