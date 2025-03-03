using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class SceneController : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debug;
    [SerializeField] private float debugDelay;
    
    private LoadingScreen loadingScreen;
    private static SceneController instance;
    public static SceneController Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("SceneController");
                instance = go.AddComponent<SceneController>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Get loading screen reference from UIManager
        if (UIManager.Instance != null)
        {
            HandleUIManagerReady();
        }
    }

    // Subscribe to UIManager when it's created
    private void OnEnable()
    {
        UIManager.OnUIManagerReady += HandleUIManagerReady;
    }

    private void OnDisable()
    {
        UIManager.OnUIManagerReady -= HandleUIManagerReady;
    }

    private void HandleUIManagerReady()
    {
        loadingScreen = UIManager.Instance.GetLoadingScreen();
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
            loadingScreen.ShowLoadingScreen(0f);

        // Start loading the scene
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;

        // While the scene is loading
        while (!asyncOperation.isDone)
        {
            // Calculate progress
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            // Update loading screen
            if (loadingScreen != null)
                loadingScreen.UpdateProgress(progress);

            // Check if loading is complete
            if (asyncOperation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(debug ? debugDelay : 0.5f); // Optional delay
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        // Hide loading screen
        if (loadingScreen != null)
            loadingScreen.HideLoadingScreen();
    }

    private IEnumerator LoadLevelAsync(int sceneIndex)
    {
        // Show loading screen
        if (loadingScreen != null)
            loadingScreen.ShowLoadingScreen(0f);

        // Start loading the scene
        AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneIndex);
        asyncOperation.allowSceneActivation = false;

        // While the scene is loading
        while (!asyncOperation.isDone)
        {
            // Calculate progress
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            // Update loading screen
            if (loadingScreen != null)
                loadingScreen.UpdateProgress(progress);

            // Check if loading is complete
            if (asyncOperation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(debug ? debugDelay : 0.5f); // Optional delay
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        // Hide loading screen
        if (loadingScreen != null)
            loadingScreen.HideLoadingScreen();
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
