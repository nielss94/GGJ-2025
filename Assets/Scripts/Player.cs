using System;
using UnityEngine;
using DG.Tweening;
using StarterAssets;

public class Player : MonoBehaviour
{
    [Header("Warp Effect Settings")]
    [SerializeField] private float maxWarpFOV = 130f;
    [SerializeField] private float minWarpFOVIncrease = 50f;
    [SerializeField] private float maxWarpDistance = 10f;
    [SerializeField] private float minWarpDuration = 0.3f;
    [SerializeField] private float maxWarpDuration = 0.4f;
    [SerializeField] private bool rotateTowardsDestination = true;
    [SerializeField] private bool resetXRotation = true;
    
    [Header("References")]
    [SerializeField] private Transform cameraRoot;
    
    private FirstPersonController firstPersonController;
    public event Action OnPlayerDeath;
    private bool isDead = false;
    private CharacterController characterController;
    private Camera mainCamera;

    void Awake() {
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        firstPersonController = GetComponent<FirstPersonController>();
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
        
        var moveSequence = DOTween.Sequence()
            .Join(transform.DOMove(position, moveDuration).SetEase(Ease.InOutQuint))
            .Join(rotateTowardsDestination && (position - transform.position).normalized != Vector3.zero ? 
                transform.DORotate(new Vector3(0, Quaternion.LookRotation((position - transform.position).normalized).eulerAngles.y, 0), moveDuration) : 
                null);

        if (resetXRotation && cameraRoot != null)
        {
            moveSequence.Join(cameraRoot.DOLocalRotate(new Vector3(0, 0, 0), moveDuration));
            // Set the private field via reflection since we can't access it directly
            var field = typeof(FirstPersonController).GetField("_cinemachineTargetPitch", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null && firstPersonController != null)
            {
                field.SetValue(firstPersonController, 0f);
            }
        }
        
        sequence
            .SetEase(Ease.InOutQuad)
            // Warp out effect
            .Append(mainCamera.DOFieldOfView(warpFOV, moveDuration * 0.3f).SetEase(Ease.InExpo))
            // Disable character controller during move
            .AppendCallback(() => characterController.enabled = false)
            // Quick move to target and rotate if enabled
            .Append(moveSequence)
            // Re-enable character controller after move
            .AppendCallback(() => characterController.enabled = true)
            // Warp in effect
            .Append(mainCamera.DOFieldOfView(originalFOV, moveDuration * 0.5f).SetEase(Ease.OutExpo))
            .AppendCallback(() => Events.Rebirth());
    }
}
