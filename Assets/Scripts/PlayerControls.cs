using StarterAssets;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{

    [SerializeField]
    private HandCannon handCannon;

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
}
