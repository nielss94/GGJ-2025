using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public GameObject container;
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    
    private void Awake()
    {
        container.SetActive(false);
    }

    public void ShowLoadingScreen(float progress)
    {
        container.SetActive(true);
        UpdateProgress(progress);
    }

    public void UpdateProgress(float progress)
    {
        if (progressBar != null)
            progressBar.value = progress;
        if (progressText != null)
            progressText.text = $"Loading... {progress * 100:0}%";
    }

    public void HideLoadingScreen()
    {
        container.SetActive(false);
    }
}