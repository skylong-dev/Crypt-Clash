
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Damage player = collision.GetComponentInParent<Damage>();

        if (player != null)
        {
            player.LoseStock();
        }
    }
}
