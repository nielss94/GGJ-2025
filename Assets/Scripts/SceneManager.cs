using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class SceneManager : MonoBehaviour
{
    private static SceneManager instance;
    public static SceneManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("SceneManager");
                instance = go.AddComponent<SceneManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private GameObject loadingScreen;
    private Slider progressBar;
    private TextMeshProUGUI progressText;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (UIManager.Instance != null) {
            loadingScreen = UIManager.Instance.GetLoadingScreen();
        }
            
    }

    public void LoadLevel(string sceneName)
    {
        StartCoroutine(LoadLevelAsync(sceneName));
    }

    public void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadLevelAsync(sceneIndex));
    }

    private IEnumerator LoadLevelAsync(string sceneName)
    {
        // Show loading screen
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        // Start loading the scene
        AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;

        // While the scene is loading
        while (!asyncOperation.isDone)
        {
            // Calculate progress
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            // Update UI elements if they exist
            if (progressBar != null)
                progressBar.value = progress;
            if (progressText != null)
                progressText.text = $"Loading... {progress * 100:0}%";

            // Check if loading is complete
            if (asyncOperation.progress >= 0.9f)
            {
                // Wait for any additional conditions if needed
                yield return new WaitForSeconds(0.5f); // Optional delay
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        // Hide loading screen
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }

    private IEnumerator LoadLevelAsync(int sceneIndex)
    {
        // Show loading screen
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        // Start loading the scene
        AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneIndex);
        asyncOperation.allowSceneActivation = false;

        // While the scene is loading
        while (!asyncOperation.isDone)
        {
            // Calculate progress
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            // Update UI elements if they exist
            if (progressBar != null)
                progressBar.value = progress;
            if (progressText != null)
                progressText.text = $"Loading... {progress * 100:0}%";

            // Check if loading is complete
            if (asyncOperation.progress >= 0.9f)
            {
                // Wait for any additional conditions if needed
                yield return new WaitForSeconds(0.5f); // Optional delay
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        // Hide loading screen
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }

    public int GetCurrentSceneIndex()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
    }

    public string GetCurrentSceneName()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }
}
