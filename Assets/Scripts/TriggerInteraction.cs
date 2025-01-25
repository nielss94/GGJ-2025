using UnityEngine;
using UnityEngine.Events;

public class TriggerInteraction : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onTriggerEnter;
    [SerializeField]
    private UnityEvent onTriggerExit;
    private void OnTriggerEnter(Collider other)
    {
        onTriggerEnter?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        onTriggerExit?.Invoke();
    }
}
