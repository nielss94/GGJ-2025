using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public event Action OnPlayerDeath;
    private bool isDead = false;
    private CharacterController characterController;

    void Awake() {
        characterController = GetComponent<CharacterController>();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        
        Debug.Log("Player died!");
        OnPlayerDeath?.Invoke();
    }

    public void Respawn(Vector3 spawnPosition)
    {
        TeleportPlayer(spawnPosition);
        isDead = false;
    }

    public void TeleportPlayer(Vector3 position) {
        characterController.enabled = false;
        transform.position = position;
        characterController.enabled = true;
        Events.Rebirth();
    }
}
