using UnityEngine;

public class Damage : MonoBehaviour
{
    public int maxHealth = 100;
    public int health = 100;

    [Header("Player Settings")]
    public int playerNumber = 1;
    public Transform respawnPoint;

    private bool defeated = false;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        health = maxHealth;
    }

    public void InitializeForMatch(int slot, Transform spawnPoint)
    {
        playerNumber = slot;
        respawnPoint = spawnPoint;
    }

    public void TakeDamage(int damage)
    {
        if (defeated)
        {
            return;
        }

        health -= damage;
        health = Mathf.Max(0, health);

        Debug.Log(
            gameObject.name + " took " + damage + " damage."
        );

        if (health <= 0)
        {
            defeated = true;

            if (MatchManager.Instance != null)
            {
                MatchManager.Instance.PlayerLostStock(playerNumber);
            }
            else
            {
                Debug.LogWarning("No MatchManager found.");
            }
        }
    }

    public void RespawnPlayer()
    {
        GetComponentInParent<Combat>()?.ClearHitStun();

        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
        }
        else
        {
            Debug.LogWarning(
                gameObject.name + " has no respawn point assigned."
            );
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        health = maxHealth;
        defeated = false;
    }

    public int GetHealth()
    {
        return health;
    }

    public bool IsDefeated()
    {
        return defeated;
    }
}
