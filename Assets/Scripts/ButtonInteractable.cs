using DG.Tweening;
using UnityEngine;

public class ButtonInteractable : MonoBehaviour
{
    [SerializeField]
    private GameObject button;

    private bool pressed = false;

    private Vector3 originalPosition;

    [SerializeField]
    private Color pressedColor;
    [SerializeField]
    private Color unpressedColor;

    [SerializeField]
    private Renderer buttonRenderer;

    [SerializeField]
    private Light buttonLight;

    private void Start() {
        originalPosition = button.transform.position;
    }

    public void Toggle() {
        pressed = !pressed;
        button.transform.DOMoveY(pressed ? originalPosition.y - 0.05f : originalPosition.y, 0.1f).SetEase(Ease.OutBack);
        buttonRenderer.material.SetColor("_EmissiveColor", pressed ? pressedColor : unpressedColor);
        buttonLight.color = pressed ? pressedColor : unpressedColor;
    }
}
