using System;
using DG.Tweening;
using StarterAssets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class HandCannon : MonoBehaviour
{
    public UnityEvent OnFire;
    public UnityEvent OnTeleport;
    public UnityEvent OnCancel;
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
    private Player player;

    private bool canShoot = true;

    [SerializeField] private SkinnedMeshRenderer arms;

    private bool initialTeleportFinished = false;

    private void Awake() {
        arms.enabled = false;
        player.GetComponent<CharacterController>().enabled = false;
        if (!activeEgg) {
            activeEgg = Instantiate(eggPrefab, firePoint.position + player.transform.forward.normalized * 2 + player.transform.up.normalized * 2, firePoint.rotation);
            eggActivationTime = Time.time;
            activeEgg.OnBreak += OnEggBreak;
        } else {
            activeEgg.OnBreak += OnEggBreak;
        }
    }

    public void Fire()
    {
        if (activeEgg)
        {
            Debug.Log("Cant fire, egg is already alive");
            return;
        }

        if (!canShoot)
        {
            Debug.Log("Cant fire, canShoot is false");
            return;
        }

        activeEgg = Instantiate(eggPrefab, firePoint.position, firePoint.rotation);
        activeEgg.Launch(bulletForce);
        eggActivationTime = Time.time;
        activeEgg.OnBreak += OnEggBreak;
        OnFire?.Invoke();
    }

    public void Teleport()
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

        if (!activeEgg.CanRebirth)
        {
            Debug.Log("Egg is not ready to rebirth");
            return;
        }

        // initiate egg swap
        initialTeleportFinished = true;
        activeEgg.Break(() => {
            player.TeleportPlayer(activeEgg.transform.position);
            arms.enabled = true;
            OnTeleport?.Invoke();
        });
    }

    public void Cancel()
    {
        if (!activeEgg)
        {
            Debug.Log("Cant cancel, egg is not alive");
            return;
        }

        activeEgg.Break();
        OnCancel?.Invoke();
    }

    public void SetCanShoot(bool canShoot)
    {
        this.canShoot = canShoot;
    }

    void OnEggBreak()
    {
        if (!activeEgg)
        {
            Debug.Log("Cant break, egg is not alive");
            return;
        }

        if (!initialTeleportFinished) {
            return;
        }

        activeEgg.OnBreak -= OnEggBreak;
        Destroy(activeEgg.gameObject);
        activeEgg = null;
    }

}
