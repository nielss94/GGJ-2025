using StarterAssets;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [SerializeField]
    private PlayerInteraction playerInteraction;

    [SerializeField]
    private HandCannon handCannon;

    void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetCanShoot(bool canShoot) {
        handCannon.SetCanShoot(canShoot);
    }

    void OnFire()
    {
        handCannon.Fire();
    }

    void OnCancel()
    {
        handCannon.Cancel();
    }

    void OnTeleport()
    {
        handCannon.Teleport();
    }

    void OnInteract()
    {
        playerInteraction.Interact();
    }
}
