using UnityEngine;
using UnityEngine.Events;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField]
    protected UnityEvent onInteract;
    public virtual void Interact()
    {
        onInteract?.Invoke();
    }
    public string interactText;
}
