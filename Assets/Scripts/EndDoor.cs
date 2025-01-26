using UnityEngine;
using DG.Tweening;

public class EndDoor : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private Ease moveEase = Ease.InOutQuad;
    [SerializeField] private float rotations = 2f;

    private bool levelEnded = false;
    void Start()
    {
        player = FindFirstObjectByType<Player>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Egg egg) && !levelEnded)
        {
            player.DisableControls();
            egg.DisableMovement();
            levelEnded = true;
            
            Sequence sequence = DOTween.Sequence();
            sequence.Join(egg.transform.DOMove(endPoint.position, moveDuration).SetEase(moveEase));
            sequence.Join(egg.transform.DORotate(new Vector3(0, 360f * rotations, 0), moveDuration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear));

            sequence.AppendCallback(() => {
                player.TeleportPlayer(endPoint.position - Vector3.up);
                egg.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
                egg.transform.DOMove(endPoint.position + Vector3.right * 2f, 0.5f).SetEase(Ease.InBack);
            });
        }

        if (other.TryGetComponent(out Player playerCollider) && !levelEnded) {
            playerCollider.TeleportPlayer(endPoint.position - Vector3.up);
        }
    }
}
