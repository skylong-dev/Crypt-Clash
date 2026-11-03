using UnityEngine;

public class Knockback : MonoBehaviour
{
    public float defaultKnockback = 5f;
    public float upwardForce = 2f;

    private Rigidbody2D rb;

    // Stops the character from getting knocked back
    // over and over before recovering.
    private bool canBeKnockedBack = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogWarning("No Rigidbody2D found on " + gameObject.name);
        }
    }

    public void ApplyKnockback(
        float horizontalForce,
        float verticalForce,
        bool attackerFacingRight)
    {
        if (rb == null)
        {
            return;
        }

        if (!canBeKnockedBack)
        {
            return;
        }

        float direction;

        if (attackerFacingRight)
        {
            direction = 1f;
        }
        else
        {
            direction = -1f;
        }

        Vector2 knockbackAmount = new Vector2(
            horizontalForce * direction,
            verticalForce
        );

        // Reset some of the current movement first
        // so the knockback is easier to notice.
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        rb.AddForce(
            knockbackAmount,
            ForceMode2D.Impulse
        );

        Debug.Log("Knockback applied to " + gameObject.name);
    }

    public void ResetKnockback()
    {
        canBeKnockedBack = true;
    }

    public void DisableKnockback()
    {
        canBeKnockedBack = false;
    }
}