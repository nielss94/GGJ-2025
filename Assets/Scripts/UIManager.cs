using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI interactText;

    [SerializeField]
    private TextMeshProUGUI eggVelocity;

    public static UIManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void SetInteractText(string text)
    {
        interactText.text = text;
    }

    public void SetEggVelocity(float velocity)
    {
        eggVelocity.text = $"{velocity:F2} m/s";
    }
}
