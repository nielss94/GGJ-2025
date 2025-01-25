using UnityEngine;

public class TestInteractable : Interactable
{
    public override void Interact()
    {
        Debug.Log($"Interacted with {transform.name}");
    }
}
