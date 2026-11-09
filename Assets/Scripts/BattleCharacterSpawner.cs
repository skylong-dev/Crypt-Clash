using UnityEngine;

public class BattleCharacterSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform player1SpawnPoint;
    public Transform player2SpawnPoint;

    [Header("Backup Player Prefabs")]
    public GameObject player1Prefab;
    public GameObject player2Prefab;

    private void Start()
    {
        SpawnCharacters();
    }

    private void SpawnCharacters()
    {
        // Use selected characters first.
        // If no character was selected, use the Inspector backup.
        GameObject player1ToSpawn =
            CharacterSelectionData.Player1Prefab != null
            ? CharacterSelectionData.Player1Prefab
            : player1Prefab;

        GameObject player2ToSpawn =
            CharacterSelectionData.Player2Prefab != null
            ? CharacterSelectionData.Player2Prefab
            : player2Prefab;

        // Check Player 1
        if (player1ToSpawn == null)
        {
            Debug.LogError("Player 1 prefab is missing! Assign a prefab in the BattleCharacterSpawner Inspector.");
            return;
        }

        // Check Player 2
        if (player2ToSpawn == null)
        {
            Debug.LogError("Player 2 prefab is missing! Assign a prefab in the BattleCharacterSpawner Inspector.");
            return;
        }

        // Check spawn points
        if (player1SpawnPoint == null || player2SpawnPoint == null)
        {
            Debug.LogError("Spawn points are not assigned!");
            return;
        }

        // Spawn Player 1
        GameObject spawnedPlayer1 = Instantiate(
            player1ToSpawn,
            player1SpawnPoint.position,
            Quaternion.identity
        );

        // Spawn Player 2
        GameObject spawnedPlayer2 = Instantiate(
            player2ToSpawn,
            player2SpawnPoint.position,
            Quaternion.identity
        );

        Debug.Log("Both players spawned successfully!");
    }
}