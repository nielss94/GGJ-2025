using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    private LayerMask interactableLayer;

    [SerializeField]
    private float interactDistance;

    [SerializeField]
    private bool canInteract = false;

    [SerializeField]
    private Transform interactionRoot;
    private Interactable currentInteractable;


    void Update()
    {
        if (canInteract)
        {
            if (Physics.Raycast(interactionRoot.position, interactionRoot.forward, out RaycastHit hit, interactDistance, interactableLayer))
            {
                if (hit.transform.TryGetComponent(out Interactable interactable))
                {
                    currentInteractable = interactable;
                    UIManager.Instance.SetInteractText(interactable.interactText);
                }
                else
                {
                    currentInteractable = null;
                    UIManager.Instance.SetInteractText("");
                }
            }
            else
            {
                currentInteractable = null;
                    UIManager.Instance.SetInteractText("");
            }
        }
    }

    public void Interact()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }
}
