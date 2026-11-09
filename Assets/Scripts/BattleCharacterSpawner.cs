using UnityEngine;

public class BattleCharacterSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform player1SpawnPoint;
    public Transform player2SpawnPoint;

    private void Start()
    {
        SpawnCharacters();
    }

    private void SpawnCharacters()
    {
        if (CharacterSelectionData.Player1Prefab == null)
        {
            Debug.LogError("Player 1 prefab was not saved!");
            return;
        }

        if (CharacterSelectionData.Player2Prefab == null)
        {
            Debug.LogError("Player 2 prefab was not saved!");
            return;
        }

        if (player1SpawnPoint == null || player2SpawnPoint == null)
        {
            Debug.LogError("Spawn points are not assigned!");
            return;
        }

        // Spawn Player 1
        Instantiate(
            CharacterSelectionData.Player1Prefab,
            player1SpawnPoint.position,
            Quaternion.identity
        );

        // Spawn Player 2
        Instantiate(
            CharacterSelectionData.Player2Prefab,
            player2SpawnPoint.position,
            Quaternion.identity
        );

        Debug.Log("Both players spawned successfully!");
    }
}