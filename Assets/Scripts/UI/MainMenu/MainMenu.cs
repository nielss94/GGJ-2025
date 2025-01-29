using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string nextLevelName;


    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void StartGame()
    {
        SceneController.Instance.LoadLevel(nextLevelName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
