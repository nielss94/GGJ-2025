using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public event Action OnLevelComplete;

    [SerializeField] private string NextLevelName;
    [SerializeField] private Player player;

    private List<Vector3> playerSpawnPositions = new List<Vector3>();

    void Awake()
    {
        PlayerSetup();
    }

    public void LevelComplete() {
        OnLevelComplete?.Invoke();
        SceneController.Instance.LoadLevel(NextLevelName);
    }

    public void RespawnPlayer()
    {
        // Respawn the player at the last spawn position
        player.Respawn(playerSpawnPositions[playerSpawnPositions.Count - 1]);
    }

    public void AddCheckpoint(Vector3 checkpointPosition)
    {
        playerSpawnPositions.Add(checkpointPosition);
    }

    private void PlayerSetup() 
    {
        if(player == null) {
            // Find the player in the scene
            player = FindFirstObjectByType<Player>();
            if(player == null) {
                Debug.LogError("Player not found! Make sure the player is referenced in the LevelManager.");
                return;
            }
        }
        
        player.OnPlayerDeath += RespawnPlayer;
        AddCheckpoint(player.transform.position);
    }

    public void RestartLevel() {
        SceneController.Instance.LoadLevel(SceneManager.GetActiveScene().name);
    }

    private void OnDrawGizmos()
    {
        // Only draw if we have spawn positions
        if (playerSpawnPositions != null && playerSpawnPositions.Count > 0)
        {
            // Draw a sphere at the latest spawn position
            Gizmos.color = Color.green;
            Vector3 latestSpawnPos = playerSpawnPositions[playerSpawnPositions.Count - 1];
            Gizmos.DrawWireSphere(latestSpawnPos, 1f);
        }
    }
}
