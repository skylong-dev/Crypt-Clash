using UnityEngine;
using UnityEngine.InputSystem;

public enum AttackType
{
    Light,
    Heavy
}

public enum AttackVariation
{
    Neutral,
    Forward,
    Backward,
    Up,
    Down
}

[System.Serializable]
public class AttackSettings
{
    public AttackType attackType;
    public AttackVariation variation;
    public bool isAirAttack;

    [Header("Damage")]
    public int damage = 10;

    [Header("Hitbox")]
    public float range = 1.2f;
    public Vector2 hitboxSize = new Vector2(1.2f, 1f);

    [Header("Knockback")]
    public float horizontalKnockback = 5f;
    public float verticalKnockback = 2f;
}

public class Combat : MonoBehaviour
{
    [Header("Attack Settings")]
    public AttackSettings[] attackSettings = new AttackSettings[18];
    public float attackCooldown = 0.4f;

    [Header("Attack Animations")]
    [SerializeField] private Animator animator;

    private float nextAttackTime;

    [Header("Facing Direction")]
    public bool facingRight = true;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool isGrounded;
    private float horizontalInput;
    private float verticalInput;

    // Player 1 uses the first connected controller.
    private Gamepad player1Controller;

    private void Awake()
    {
        // The Animator can live on a child such as SlimeVisual.
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator == null)
            Debug.LogWarning("Combat: No Animator found. Assign the SlimeVisual Animator in the Inspector.");
    }

    private void Update()
    {
        UpdatePlayer1Controller();
        ReadDirectionInput();
        UpdateGrounded();

        if (Time.time < nextAttackTime)
            return;

        // Keyboard controls.
        if (Keyboard.current != null)
        {
            // J = Light Attack.
            if (Keyboard.current.jKey.wasPressedThisFrame)
            {
                LightAttack();
                return;
            }

            // K = Heavy Attack.
            if (Keyboard.current.kKey.wasPressedThisFrame)
            {
                HeavyAttack();
                return;
            }
        }

        // Player 1 controller:
        // X (Xbox) / Square (PlayStation) = Light Attack.
        // No controller button is assigned to Heavy Attack.
        if (player1Controller != null &&
            player1Controller.buttonWest.wasPressedThisFrame)
        {
            LightAttack();
        }
    }

    private void UpdatePlayer1Controller()
    {
        if (Gamepad.all.Count > 0)
            player1Controller = Gamepad.all[0];
        else
            player1Controller = null;
    }

    private void ReadDirectionInput()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        // Read keyboard directions.
        if (Keyboard.current != null)
        {
            bool left =
                Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed;

            bool right =
                Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed;

            bool down =
                Keyboard.current.sKey.isPressed ||
                Keyboard.current.downArrowKey.isPressed;

            bool up =
                Keyboard.current.wKey.isPressed ||
                Keyboard.current.upArrowKey.isPressed;

            if (left != right)
                horizontalInput = right ? 1f : -1f;

            if (up != down)
                verticalInput = up ? 1f : -1f;
        }

        // Controller directional input overrides keyboard input
        // when Player 1 moves the left stick or D-pad.
        if (player1Controller != null)
        {
            Vector2 stick =
                player1Controller.leftStick.ReadValue();

            Vector2 dpad =
                player1Controller.dpad.ReadValue();

            Vector2 controllerInput = dpad;

            if (stick.magnitude > 0.25f)
                controllerInput = stick;

            if (controllerInput.magnitude > 0.25f)
            {
                horizontalInput =
                    Mathf.Abs(controllerInput.x) > 0.25f
                    ? Mathf.Sign(controllerInput.x)
                    : 0f;

                verticalInput =
                    Mathf.Abs(controllerInput.y) > 0.25f
                    ? Mathf.Sign(controllerInput.y)
                    : 0f;
            }
        }
    }

    private void UpdateGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void LightAttack()
    {
        PerformAttack(AttackType.Light);
    }

    private void HeavyAttack()
    {
        PerformAttack(AttackType.Heavy);
    }

    private void PerformAttack(AttackType type)
    {
        bool airAttack = !isGrounded;


        AttackVariation variation =
            GetAttackVariation(airAttack);

        AttackSettings settings = null;

        foreach (AttackSettings item in attackSettings)
        {
            if (item == null)
                continue;

            if (item.attackType == type &&
                item.variation == variation &&
                item.isAirAttack == airAttack)
            {
                settings = item;
                break;
            }
        }

        if (settings == null)
        {
            Debug.LogWarning(
                "Missing attack settings: " + type + " " +
                variation + (airAttack ? " Air" : " Ground")
            );

            return;
        }

        nextAttackTime = Time.time + attackCooldown;

        Debug.Log(
            (airAttack ? "Air " : "Ground ") +
            type + " Attack: " + variation
        );

        if (type == AttackType.Light)
            PlayLightAttackAnimation(variation, airAttack);

        CheckForEnemies(settings);
    }

    private void PlayLightAttackAnimation(AttackVariation variation, bool airAttack)
    {
        if (animator == null)
            return;

        // Examples: LightGroundNeutral, LightGroundForward, LightAirDown.
        string triggerName = "Light" + (airAttack ? "Air" : "Ground") + variation;
        animator.SetTrigger(triggerName);
    }

    private AttackVariation GetAttackVariation(bool airAttack)
    {
        // Up and Down take priority.
        if (verticalInput > 0f)
            return AttackVariation.Up;

        if (verticalInput < 0f)
            return AttackVariation.Down;

        if (horizontalInput == 0f)
            return AttackVariation.Neutral;

        // Determine Forward and Backward relative to facing.
        bool pressingForward =
            (horizontalInput > 0f && facingRight) ||
            (horizontalInput < 0f && !facingRight);

        if (pressingForward)
            return AttackVariation.Forward;

        // Backward attacks are available in the air.
        // Ground backward input falls back to Neutral.
        if (airAttack)
            return AttackVariation.Backward;

        return AttackVariation.Neutral;
    }

    private void CheckForEnemies(AttackSettings settings)
    {
        float direction = facingRight ? 1f : -1f;

        Vector2 hitboxCenter =
            (Vector2)transform.position +
            new Vector2(direction * settings.range * 0.5f, 0f);

        if (settings.variation == AttackVariation.Up)
        {
            hitboxCenter.y += settings.hitboxSize.y * 0.5f;
        }
        else if (settings.variation == AttackVariation.Down)
        {
            hitboxCenter.y -= settings.hitboxSize.y * 0.5f;
        }

        Collider2D[] enemies = Physics2D.OverlapBoxAll(
            hitboxCenter,
            settings.hitboxSize,
            0f
        );

        foreach (Collider2D enemy in enemies)
        {
            if (!enemy.CompareTag("Enemy"))
                continue;

            Damage damageScript = enemy.GetComponent<Damage>();

            if (damageScript == null)
                damageScript = enemy.GetComponentInParent<Damage>();

            if (damageScript == null)
                continue;

            damageScript.TakeDamage(settings.damage);

            Knockback knockback = enemy.GetComponent<Knockback>();

            if (knockback == null)
                knockback = enemy.GetComponentInParent<Knockback>();

            if (knockback != null)
            {
                knockback.ApplyKnockback(
                    settings.horizontalKnockback,
                    settings.verticalKnockback,
                    facingRight
                );
            }
        }
    }

    // Call this from the movement script when the player turns.
    public void SetFacingDirection(bool lookingRight)
    {
        facingRight = lookingRight;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }
    }
}