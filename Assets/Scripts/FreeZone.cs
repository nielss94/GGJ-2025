using Unity.VisualScripting;
using UnityEngine;

public class FreeZone : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debug;

    [SerializeField]
    private bool rebirthFreeZone;

    [SerializeField]
    private bool shootingFreeZone;

    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = debug;
    }

    void OnTriggerEnter(Collider other)
    {
        if (rebirthFreeZone && other.TryGetComponent<Egg>(out Egg egg))
        {
            egg.SetInRebirthFreeZone(true);
        }

        if (shootingFreeZone && other.TryGetComponent<PlayerControls>(out PlayerControls player))
        {
            player.SetCanShoot(false);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (rebirthFreeZone && other.TryGetComponent<Egg>(out Egg egg))
        {
            egg.SetInRebirthFreeZone(false);
        }

        if (shootingFreeZone && other.TryGetComponent<PlayerControls>(out PlayerControls player))
        {
            player.SetCanShoot(true);
        }
    }
}
