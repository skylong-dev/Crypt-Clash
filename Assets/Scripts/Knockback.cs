using UnityEngine;

public class Knockback : MonoBehaviour
{
    public float defaultKnockback = 5f;
    public float upwardForce = 2f;

    private Rigidbody2D rb;

    // Other scripts can temporarily disable knockback.
    private bool canBeKnockedBack = true;

    private void Awake()
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

        float direction = attackerFacingRight ? 1f : -1f;
        Vector2 impulse = new Vector2(horizontalForce * direction, verticalForce);

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(impulse, ForceMode2D.Impulse);

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
