using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string nextLevelName;
   
    public void StartGame() {
        SceneController.Instance.LoadLevel(nextLevelName);
    }
}
