using System;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class UIManager : MonoBehaviour
{
    public static event Action OnUIManagerReady;
    public static UIManager Instance { get; private set; }

    [SerializeField]
    private TextMeshProUGUI interactText;
    [Header("Loading Screen")]
    [SerializeField] private LoadingScreen loadingScreen;

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

    private void Start()
    {
        OnUIManagerReady?.Invoke();
    }

    public void SetInteractText(string text)
    {
        interactText.text = text;
    }

    public LoadingScreen GetLoadingScreen()
    {
        return loadingScreen;
    }
}
