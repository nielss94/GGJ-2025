using System;
using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour
{
    [Header("Warp Effect Settings")]
    [SerializeField] private float maxWarpFOV = 130f;
    [SerializeField] private float minWarpFOVIncrease = 50f;
    [SerializeField] private float maxWarpDistance = 10f;
    [SerializeField] private float minWarpDuration = 0.3f;
    [SerializeField] private float maxWarpDuration = 0.4f;
    
    public event Action OnPlayerDeath;
    private bool isDead = false;
    private CharacterController characterController;

    private Camera mainCamera;

    void Awake() {
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
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
        var sequence = DOTween.Sequence();
        
        float originalFOV = mainCamera.fieldOfView;
        
        // Calculate distance and use it to scale the effect
        float distance = Vector3.Distance(transform.position, position);
        float distanceScale = Mathf.Clamp01(distance / maxWarpDistance);
        
        float warpFOV = Mathf.Lerp(originalFOV + minWarpFOVIncrease, maxWarpFOV, distanceScale);
        float moveDuration = Mathf.Lerp(minWarpDuration, maxWarpDuration, distanceScale);
        
        sequence
            .SetEase(Ease.InOutQuad)
            // Warp out effect
            .Append(mainCamera.DOFieldOfView(warpFOV, moveDuration * 0.3f).SetEase(Ease.InExpo))
            // Disable character controller during move
            .AppendCallback(() => characterController.enabled = false)
            // Quick move to target
            .Append(transform.DOMove(position, moveDuration).SetEase(Ease.InOutQuint))
            // Re-enable character controller after move
            .AppendCallback(() => characterController.enabled = true)
            // Warp in effect
            .Append(mainCamera.DOFieldOfView(originalFOV, moveDuration * 0.5f).SetEase(Ease.OutExpo))
            .AppendCallback(() => Events.Rebirth());
    }
}
