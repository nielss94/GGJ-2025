using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
   
    public void StartGame() {
        SceneController.Instance.LoadLevel("Level_01");
    }
}
