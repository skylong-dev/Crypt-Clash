
using UnityEngine;

public class BattlesCharacterSpawner : MonoBehaviour
{
    [Header("Player Prefabs")]
    public GameObject player1Prefab;
    public GameObject player2Prefab;

    [Header("Spawn Positions")]
    public Transform player1Spawn;
    public Transform player2Spawn;

    // Spawn a player when the match starts or they lose a stock
    public Damage SpawnPlayer(int playerNumber)
    {
        GameObject prefab = null;
        Transform spawnPoint = null;

        if (playerNumber == 1)
        {
            prefab = player1Prefab;
            spawnPoint = player1Spawn;
        }
        else if (playerNumber == 2)
        {
            prefab = player2Prefab;
            spawnPoint = player2Spawn;
        }
        else
        {
            Debug.LogError("Invalid player number!");
            return null;
        }

        // Check references
        if (prefab == null)
        {
            Debug.LogError("Player " + playerNumber + " prefab is missing!");
            return null;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Player " + playerNumber + " spawn point is missing!");
            return null;
        }

        // Create player
        GameObject newPlayer = Instantiate(
            prefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // Get Damage component
        Damage damage = newPlayer.GetComponent<Damage>();

        if (damage == null)
        {
            Debug.LogError("Player prefab needs a Damage script!");
            Destroy(newPlayer);
            return null;
        }

        // Initialize player
        damage.InitializeForMatch(playerNumber, spawnPoint);

        Debug.Log("Player " + playerNumber + " spawned!");

        return damage;
    }
}
