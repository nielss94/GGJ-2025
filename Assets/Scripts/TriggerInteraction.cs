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
    [SerializeField]
    private bool triggerOnce = false;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;
        
        if (layer == (layer | (1 << other.gameObject.layer)))
        {
            onTriggerEnter?.Invoke();
            if (triggerOnce)
            {
                hasTriggered = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (hasTriggered && triggerOnce) return;

        if (layer == (layer | (1 << other.gameObject.layer)))
        {
            onTriggerExit?.Invoke();
            if (triggerOnce)
            {
                hasTriggered = true;
            }
        }
    }
}
