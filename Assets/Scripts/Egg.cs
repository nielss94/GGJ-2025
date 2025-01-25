using UnityEngine;
using System;
using UnityEngine.PlayerLoop;
using UnityEngine.Events;
using System.Collections;
using DG.Tweening;

public class Egg : MonoBehaviour
{
    public UnityEvent OnEggBreak;
    public event Action OnBreak;

    [SerializeField]
    private bool canRebirth = true;
    public bool CanRebirth { get { return canRebirth; } }

    private Rigidbody rb;

    [SerializeField]
    private float breakSpeedThreshold = 0.1f;
    private bool isBreaking = false;
    public bool IsBreaking { get { return isBreaking; } }
    private float aliveTime = 0f;

    [SerializeField]
    private float minimumTimeBeforeSpeedCheck = 0.2f;

    [SerializeField]
    private float breakAnimationDuration = 0.2f;
    
    [SerializeField]
    private float scaleIncrease = 2.5f;

    [SerializeField]
    private Ease scaleEase = Ease.OutBack;
    
    [SerializeField]
    private Ease blendShapeEase = Ease.InOutQuad;

    private bool inRebirthFreeZone = false;

    private SkinnedMeshRenderer meshRenderer;
    private Sequence breakSequence;

    private MeshRenderer[] bubbles;

    private bool animating = false;
    public bool Animating { get { return animating; } }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        aliveTime = 0f;
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        bubbles = GetComponentsInChildren<MeshRenderer>(true);
    }

    private void OnDestroy()
    {
        breakSequence?.Kill();
    }

    public void AnimateAndBreak()
    {
        if (animating) return;
        animating = true;
        AnimateBreaking(() => {
            Break();
        });
    }

    public void Break()
    {
        if (isBreaking) return;
        isBreaking = true;
        OnEggBreak?.Invoke();
        OnBreak?.Invoke();

        Events.EggSpeed(0f);
        isBreaking = false;
    }

    private void AnimateBreaking(Action onBreak)
    {
        breakSequence?.Kill();
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * scaleIncrease;
        Vector3 originalPosition = transform.position;

        // Check if we're on the ground using a small raycast
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1f);
        Vector3 targetPosition = originalPosition;
        
        if (isGrounded)
        {
            // Only adjust height if we're on the ground
            float heightIncrease = (targetScale.y - originalScale.y) * 0.5f;
            targetPosition = originalPosition + Vector3.up * heightIncrease;
        }

        // Disable physics during animation
        rb.isKinematic = true;
        if (rb.collisionDetectionMode == CollisionDetectionMode.Continuous)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        breakSequence = DOTween.Sequence();
        for (int stage = 0; stage < 4; stage++)
        {
            float stageProgress = (stage + 1) / 4f;
            Vector3 stageTargetScale = Vector3.Lerp(originalScale, targetScale, stageProgress);
            Vector3 stageTargetPosition = Vector3.Lerp(originalPosition, targetPosition, stageProgress);

            // Blend shape animation
            int blendShapeIndex = stage;
            Debug.Log($"Stage {stage}, BlendShape Index: {blendShapeIndex}");
            breakSequence.Join(
                DOTween.To(
                    () => meshRenderer.GetBlendShapeWeight(blendShapeIndex),
                    x => meshRenderer.SetBlendShapeWeight(blendShapeIndex, x),
                    0f,
                    breakAnimationDuration
                ).SetEase(blendShapeEase)
            );

            // Scale and position animation
            breakSequence.Join(
                transform.DOScale(stageTargetScale, breakAnimationDuration)
                    .SetEase(scaleEase)
            );
            
            if (isGrounded)
            {
                breakSequence.Join(
                    transform.DOMove(stageTargetPosition, breakAnimationDuration)
                        .SetEase(scaleEase)
                );
            }

            // After first stage, fade out bubbles
            if (stage == 0 && bubbles != null)
            {
                foreach (var bubble in bubbles)
                {
                    if (bubble != null && bubble.TryGetComponent<MeshRenderer>(out var renderer))
                    {
                        // Fade out the material's alpha
                        var material = renderer.material;
                        var color = material.color;
                        breakSequence.Join(
                            DOTween.To(
                                () => color.a,
                                x => {
                                    color.a = x;
                                    material.color = color;
                                },
                                0f,
                                breakAnimationDuration
                            ).SetEase(Ease.InQuad)
                        );
                    }
                }
            }

            if (stage < 3)
            {
                breakSequence.AppendInterval(0.1f);
            }
        }

        breakSequence.OnComplete(() => {
            onBreak?.Invoke();
        });
    }

    public void Launch(float bulletForce)
    {
        rb.AddForce(transform.forward * bulletForce, ForceMode.Impulse);
    }

    void Update()
    {
        if (rb == null || UIManager.Instance == null) return;
        Events.EggSpeed(rb.linearVelocity.magnitude);

        aliveTime += Time.deltaTime;

        if (rb.linearVelocity.magnitude < breakSpeedThreshold && !isBreaking && aliveTime > minimumTimeBeforeSpeedCheck && inRebirthFreeZone)
        {
            isBreaking = true;
            Break();
        }
    }

    public void SetInRebirthFreeZone(bool v)
    {
        inRebirthFreeZone = v;
        canRebirth = !v;
    }

    
}