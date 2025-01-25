using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private HandCannon handCannon;
    
    [Header("Animator Parameters")]
    [SerializeField] private string moveSpeedParamName = "moveSpeed";
    [SerializeField] private string shootParamName = "shoot";
    [SerializeField] private string cancelParamName = "cancel";
    [SerializeField] private string teleportParamName = "teleport";
    [SerializeField] private string reloadParamName = "reload";

    private void Awake()
    {       
        if (animator == null)
        {
            Debug.LogError("PlayerAnimator requires an Animator component!");
        }
        
        if (characterController == null)
        {
            Debug.LogError("PlayerAnimator requires a CharacterController component!");
        }

        if (handCannon == null)
        {
            Debug.LogError("PlayerAnimator requires a HandCannon component!");
        }

        handCannon.OnFire.AddListener(TriggerShoot);
        handCannon.OnCancel.AddListener(TriggerCancel);
        handCannon.OnTeleport.AddListener(TriggerTeleport);

    }

    private void Update()
    {
        // Get horizontal velocity (ignoring vertical movement)
        Vector3 horizontalVelocity = characterController.velocity;
        horizontalVelocity.y = 0f;  // Remove vertical component
        
        float speed = horizontalVelocity.magnitude;
        SetMovementSpeed(speed);
    }

    public void SetMovementSpeed(float speed)
    {
        animator.SetFloat(moveSpeedParamName, speed);
    }

    public void TriggerShoot()
    {
        animator.SetTrigger(shootParamName);
    }

    public void TriggerCancel()
    {
        animator.SetTrigger(cancelParamName);
    }

    public void TriggerTeleport()
    {
        animator.SetTrigger(teleportParamName);
    }

    public void TriggerReload()
    {
        animator.SetTrigger(reloadParamName);
    }
}
