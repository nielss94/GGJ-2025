using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class HandCannon : MonoBehaviour
{
    [Header("Firing options")]
    [SerializeField]
    private float eggTeleportTime;
    private float eggActivationTime = 1f;
    
    [Header("Egg options")]
    [SerializeField]
    private Egg activeEgg;
    [SerializeField]
    private Egg eggPrefab;
    [SerializeField]
    private float bulletForce = 10f;
    [SerializeField]
    private Transform firePoint;


    [Header("Input")]
    [SerializeField]
    private GameObject player;

    [Header("Input")]
    [SerializeField]
    private StarterAssetsInputs input;

    void Awake()
    {
        input.OnFirePressed += OnFire;
        input.OnCancelPressed += OnCancel;
        input.OnTeleportPressed += OnTeleport;
    }

    void OnDestroy()
    {
        input.OnFirePressed -= OnFire;
        input.OnCancelPressed -= OnCancel;
        input.OnTeleportPressed -= OnTeleport;
    }

    void OnFire()
    {
        if (activeEgg)
        {
            Debug.Log("Egg is already alive");
            return;
        }

        activeEgg = Instantiate(eggPrefab, firePoint.position, firePoint.rotation);
        activeEgg.Launch(bulletForce);
        eggActivationTime = Time.time;
        activeEgg.OnBreak += OnEggBreak;
    }

    void OnTeleport()
    {
        if (!activeEgg)
        {
            Debug.Log("Cant teleport, egg is not alive");
            return;
        }

        if (Time.time - eggActivationTime < eggTeleportTime)
        {
            Debug.Log("Egg is not ready to teleport");
            return;
        }

        // initiate egg swap
        activeEgg.Break(() => {
            player.GetComponent<CharacterController>().enabled = false;
            player.transform.position = activeEgg.transform.position;
            player.GetComponent<CharacterController>().enabled = true;
        });
    }

    void OnCancel()
    {
        if (!activeEgg)
        {
            Debug.Log("Cant cancel, egg is not alive");
            return;
        }

        activeEgg.Break();
    }

    void OnEggBreak()
    {
        if (!activeEgg)
        {
            Debug.Log("Cant break, egg is not alive");
            return;
        }

        activeEgg.OnBreak -= OnEggBreak;
        Destroy(activeEgg.gameObject);
        activeEgg = null;
    }
}
