using System;
using StarterAssets;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [SerializeField]
    private PlayerInteraction playerInteraction;

    [SerializeField]
    private HandCannon handCannon;

    private bool controlsEnabled = true;

    void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("PlayerControls Awake");
    }

    void Start() {
        
        UIManager.Instance.OnPauseMenuToggled += OnPauseMenuToggled;
    }

    private void OnDestroy() {
        UIManager.Instance.OnPauseMenuToggled -= OnPauseMenuToggled;
    }

    private void OnPauseMenuToggled(bool isActive)
    {
        Debug.Log("OnPauseMenuToggled: " + isActive);
        if (isActive) {
            Debug.Log("Disabling controls");
            GetComponent<Player>().DisableControls();
            controlsEnabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else {
            GetComponent<Player>().EnableControlsIfFirstTeleportOccurred();
            controlsEnabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void SetCanShoot(bool canShoot) {
        handCannon.SetCanShoot(canShoot);
    }

    void OnFire()
    {
        if (!controlsEnabled) return;
        handCannon.Fire();
    }

    void OnCancel()
    {
        if (!controlsEnabled) {
            UIManager.Instance.TogglePauseMenu();
            return;
        }
        handCannon.Cancel();
    }

    void OnTeleport()
    {
        if (!controlsEnabled) return;
        handCannon.Teleport();
    }

    void OnStart() {
        UIManager.Instance.TogglePauseMenu();
    }

    void OnInteract()
    {
        if (!controlsEnabled) return;
        playerInteraction.Interact();
    }

    public void DisableControls() {
        controlsEnabled = false;
    }
}
