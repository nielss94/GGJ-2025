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
    [SerializeField] private float minWarpDuration = 0.5f;
    [SerializeField] private float maxWarpDuration = 1.2f;
    [SerializeField] private float firstTeleportDuration = 1.0f;
    [SerializeField] private bool rotateTowardsDestination = true;
    [SerializeField] private bool resetXRotation = false;
    public bool IsTeleporting { get; private set; } = false;
    
    [Header("References")]
    [SerializeField] private Transform cameraRoot;
    
    private FirstPersonController firstPersonController;
    public event Action OnPlayerDeath;
    private bool isDead = false;
    private CharacterController characterController;
    private PlayerControls playerControls;  
    private Camera mainCamera;
    private bool hasFirstTeleportOccurred = false;
    
    [SerializeField] private Renderer armsRenderer;

    [SerializeField] private GameObject grossPuddle;

    void Awake() {
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        firstPersonController = GetComponent<FirstPersonController>();
        playerControls = GetComponent<PlayerControls>();
    }

    void Start() {
        rotateTowardsDestination = PlayerPrefs.GetInt("tpRotateHorizontal", 1) == 1;
        resetXRotation = PlayerPrefs.GetInt("tpRotateVertical", 0) == 1;
        armsRenderer.enabled = false;
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

    public void DisableControls() {
        characterController.enabled = false;
        firstPersonController.enabled = false;
        playerControls.DisableControls();
    }

    public void EnableControlsIfFirstTeleportOccurred() {
        if (hasFirstTeleportOccurred) {
            characterController.enabled = true;
        }
        firstPersonController.enabled = true;
    }

    public void TeleportPlayer(Vector3 position) {
        var sequence = DOTween.Sequence();
        
        float originalFOV = mainCamera.fieldOfView;
        
        // Calculate distance and use it to scale the effect
        float distance = Vector3.Distance(transform.position, position);
        float distanceScale = Mathf.Clamp01(distance / maxWarpDistance);
        
        // Spawn the gross puddle at the original position
        if (hasFirstTeleportOccurred) {
            Instantiate(grossPuddle, transform.position, Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0));
        }
        
        float warpFOV = Mathf.Lerp(originalFOV + minWarpFOVIncrease, maxWarpFOV, distanceScale);
        
        // Use firstTeleportDuration for the first teleport, otherwise use distance-based duration
        float moveDuration = !hasFirstTeleportOccurred ? 
            firstTeleportDuration : 
            minWarpDuration + (maxWarpDuration - minWarpDuration) * (distanceScale * distanceScale);
        
        
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

        
        IsTeleporting = true;
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
            .AppendCallback(() => IsTeleporting = false)
            // Warp in effect
            .Append(mainCamera.DOFieldOfView(originalFOV, moveDuration * 0.5f).SetEase(Ease.OutExpo))
            .AppendCallback(() => {
                Events.Rebirth();

                hasFirstTeleportOccurred = true;
            }).OnUpdate(() => {
                if (sequence.ElapsedPercentage() > 0.2f && sequence.ElapsedPercentage() < 0.6f) {
                        armsRenderer.enabled = false;
                    } else {
                        if (sequence.ElapsedPercentage() > 0.6f || (sequence.ElapsedPercentage() < 0.2f && hasFirstTeleportOccurred )) {
                            armsRenderer.enabled = true;
                        }
                    }
                AudioManager.Instance.SetTraveling(sequence.ElapsedPercentage());
            });
    }
}

