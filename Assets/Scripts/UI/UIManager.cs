using System;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class UIManager : MonoBehaviour
{
    public event Action<bool> OnPauseMenuToggled;
    public static event Action OnUIManagerReady;
    public static UIManager Instance { get; private set; }

    [SerializeField]
    private TextMeshProUGUI interactText;
    [Header("Loading Screen")]
    [SerializeField] private LoadingScreen loadingScreen;

    [SerializeField] private GameObject pauseMenu;

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

    public void TogglePauseMenu() {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        OnPauseMenuToggled?.Invoke(pauseMenu.activeSelf);
    }
}
