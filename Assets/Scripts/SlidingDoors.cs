using UnityEngine;
using DG.Tweening;
public class SlidingDoors : MonoBehaviour
{
    [SerializeField]
    private Transform doorOne;
    [SerializeField]
    private Transform doorTwo;

    [SerializeField]
    private float openDistance = 10f;

    private Vector3 originalPositionOne;
    private Vector3 originalPositionTwo;

    private void Start() {
        originalPositionOne = doorOne.localPosition;
        originalPositionTwo = doorTwo.localPosition;
    }

    public void Open() {
        doorOne.DOLocalMoveX(originalPositionOne.x - openDistance, 1f).SetEase(Ease.InOutQuint);
        doorTwo.DOLocalMoveX(originalPositionTwo.x + openDistance, 1f).SetEase(Ease.InOutQuint);
    }

    public void Close() {
        doorOne.DOLocalMoveX(originalPositionOne.x, 1f).SetEase(Ease.InOutQuint);
        doorTwo.DOLocalMoveX(originalPositionTwo.x, 1f).SetEase(Ease.InOutQuint);
    }
}
