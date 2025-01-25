using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private Player player;

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

        if (player == null)
        {
            SetPlayer(FindFirstObjectByType<Player>());
        }
    }

    public void SetPlayer(Player player) {
        this.player = player;
    }

    public Player GetPlayer() {
        return player;
    }
}
