using UnityEngine;
using System;
using UnityEngine.PlayerLoop;
using UnityEngine.Events;
using System.Collections;
using DG.Tweening;

public class Egg : MonoBehaviour
{
    public UnityEvent OnEggBreak;
    public UnityEvent OnHighVelocityCollision;
    public event Action OnBreak;

    [SerializeField]
    private bool canRebirth = true;
    public bool CanRebirth { get { return canRebirth; } }

    private Rigidbody rb;
    public Rigidbody RB { get { return rb; } }
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

    [SerializeField]
    private float highVelocityThreshold = 5f;

    [SerializeField]
    private GameObject eggSplatter;

    [SerializeField]
    private float eggSplatterInnerMetallicResult = 0.2f;
    [SerializeField]
    private Color eggSplatterInnerColor = new Color(1, 0, 0, 0.78431374f);
    [SerializeField]
    private float eggSplatterOuterMetallicResult = 0.983f;
    [SerializeField]
    private Color eggSplatterOuterColor = new Color(1, 0.5014026f, 0, 0.78431374f);
    [SerializeField]
    private Renderer eggSplatterInnerRenderer;
    [SerializeField]
    private Renderer eggSplatterOuterRenderer;

    [SerializeField]
    private float bounceForceMultiplier = 1.5f;
    [SerializeField]
    private float constantBounceForce = 10f;

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

    public void DisableMovement()
    {
        rb.isKinematic = true;
    }

    public void AnimateAndBreak()
    {
        if (animating) return;
        animating = true;
        AnimateBreaking(() =>
        {
            Break();
        });
    }
    public void Bleed()
    {
        var sequence = DOTween.Sequence();
        sequence.Join(DOTween.To(() => eggSplatterInnerRenderer.material.GetFloat("_Metallic"),
                x => eggSplatterInnerRenderer.material.SetFloat("_Metallic", x),
                eggSplatterInnerMetallicResult, 0.5f))
            .Join(DOTween.To(() => eggSplatterInnerRenderer.material.GetColor("_BaseColor"),
                x => eggSplatterInnerRenderer.material.SetColor("_BaseColor", x),
                eggSplatterInnerColor, 0.5f))
            .Join(DOTween.To(() => eggSplatterOuterRenderer.material.GetFloat("_Metallic"),
                x => eggSplatterOuterRenderer.material.SetFloat("_Metallic", x),
                eggSplatterOuterMetallicResult, 0.5f))
            .Join(DOTween.To(() => eggSplatterOuterRenderer.material.GetColor("_BaseColor"),
                x => eggSplatterOuterRenderer.material.SetColor("_BaseColor", x),
                eggSplatterOuterColor, 0.5f));
    }

    public void Break(bool splatter = false)
    {
        if (splatter)
        {

            Instantiate(eggSplatter, transform.position, eggSplatter.transform.rotation);
        }
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
                                x =>
                                {
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

        breakSequence.OnComplete(() =>
        {
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
            Break(true);
        }
    }

    public void SetInRebirthFreeZone(bool v)
    {
        inRebirthFreeZone = v;
        canRebirth = !v;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude >= highVelocityThreshold)
        {
            OnHighVelocityCollision?.Invoke();
        }

        if (collision.transform.TryGetComponent(out BouncyWall bouncyWall))
        {
            Bounce(collision.contacts[0].normal);
        }
    }

    private void Bounce(Vector3 surfaceNormal)
    {
        if (rb == null) return;

        // Calculate reflection direction using Vector3.Reflect
        Vector3 reflectionDirection = Vector3.Reflect(rb.linearVelocity.normalized, surfaceNormal);

        rb.linearVelocity = Vector3.zero; // Clear current velocity
        rb.AddForce(reflectionDirection * constantBounceForce, ForceMode.Impulse);
    }
}