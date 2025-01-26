using System;
using System.Collections;
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
    public UnityEvent OnReload;
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
    private float cancelBreakDelay = 0.5f;
    [SerializeField]
    private float bulletForce = 10f;
    [SerializeField]
    private Transform firePoint;


    [Header("Input")]
    [SerializeField]
    private Player player;
    [SerializeField]
    private FirstPersonController firstPersonController;

    private bool canShoot = true;
    private bool isFiring = false;

    private bool initialTeleportFinished = false;

    private void Awake() {
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

        if (!canShoot || isFiring)
        {
            Debug.Log("Cant fire, canShoot is false or already firing");
            return;
        }

        if (!firstPersonController.Grounded) {
            Debug.Log("Cant fire, player is not grounded");
            return;
        }
        
        OnFire?.Invoke();
        isFiring = true;
        eggActivationTime = Time.time;
        StartCoroutine(WaitAndFire());
    }

    private IEnumerator WaitAndFire() {
        yield return new WaitForSeconds(.2f);
        activeEgg = Instantiate(eggPrefab, firePoint.position, firePoint.rotation);
        activeEgg.Launch(bulletForce);
        activeEgg.OnBreak += OnEggBreak;
        isFiring = false;
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

        if (activeEgg.IsBreaking || activeEgg.Animating) {
            return;
        }

        // initiate egg swap
        initialTeleportFinished = true;
        activeEgg.AnimateAndBreak();
        player.TeleportPlayer(activeEgg.transform.position);
        OnTeleport?.Invoke();
    }

    public void Cancel()
    {
        if (!activeEgg)
        {
            Debug.Log("Cant cancel, egg is not alive");
            return;
        }
        OnCancel?.Invoke();
        StartCoroutine(WaitAndBreak());
    }

    private IEnumerator WaitAndBreak() {
        activeEgg.Bleed();
        yield return new WaitForSeconds(cancelBreakDelay);

        activeEgg.Break(true);
    }

    public void Reload()
    {
        OnReload?.Invoke();
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

        Reload();

        activeEgg.OnBreak -= OnEggBreak;
        Destroy(activeEgg.gameObject);
        activeEgg = null;
    }

}
