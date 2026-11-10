using System.Collections;
using System.Collections.Generic;
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

    private float nextAttackTime;

    [Header("Attack Animations")]
    [SerializeField] private Animator animator;

    [Header("Facing Direction")]
    public bool facingRight = true;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool isGrounded;
    private float horizontalInput;
    private float verticalInput;

    [Header("Hit Stun")]
    public float hitStunDuration = 0.5f;

    [Tooltip("Drag this player's movement script here.")]
    public MonoBehaviour movementScript;

    private bool isStunned;
    private bool movementWasEnabled;
    private Coroutine hitStunCoroutine;

    // Player 1 uses the first connected controller.
    private Gamepad player1Controller;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator == null)
        {
            Debug.LogWarning(
                "Combat: No Animator found. Assign the Animator in the Inspector."
            );
        }
    }

    private void Update()
    {
        UpdatePlayer1Controller();
        ReadDirectionInput();
        UpdateGrounded();

        // Prevent attacking while in hit stun.
        if (isStunned)
            return;

        if (Time.time < nextAttackTime)
            return;

        // Keyboard controls.
        if (Keyboard.current != null)
        {
            if (Keyboard.current.jKey.wasPressedThisFrame)
            {
                LightAttack();
                return;
            }

            if (Keyboard.current.kKey.wasPressedThisFrame)
            {
                HeavyAttack();
                return;
            }
        }

        // Player 1 controller:
        // X (Xbox) / Square (PlayStation) = Light Attack.
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

        // Controller directional input overrides keyboard input.
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
        if (isStunned)
            return;

        bool airAttack = !isGrounded;

        AttackVariation variation =
            GetAttackVariation(airAttack);

        AttackSettings settings = null;

        if (attackSettings != null)
        {
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

    private void PlayLightAttackAnimation(
        AttackVariation variation,
        bool airAttack)
    {
        if (animator == null)
            return;

        string triggerName =
            "Light" + (airAttack ? "Air" : "Ground") + variation;

        // Only use triggers that actually exist in the Animator.
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == triggerName &&
                parameter.type == AnimatorControllerParameterType.Trigger)
            {
                animator.SetTrigger(triggerName);
                return;
            }
        }

        Debug.LogWarning(
            "Animator trigger is missing: " + triggerName
        );
    }

    private AttackVariation GetAttackVariation(bool airAttack)
    {
        // Up and Down take priority.
        if (verticalInput > 0f)
            return AttackVariation.Up;

        if (verticalInput < 0f)
            return AttackVariation.Down;

        if (Mathf.Approximately(horizontalInput, 0f))
            return AttackVariation.Neutral;

        bool pressingForward =
            (facingRight && horizontalInput > 0f) ||
            (!facingRight && horizontalInput < 0f);

        if (pressingForward)
            return AttackVariation.Forward;

        // Backward attacks are only available in the air.
        if (airAttack)
            return AttackVariation.Backward;

        // Ground backward input uses the neutral attack.
        return AttackVariation.Neutral;
    }

    private void CheckForEnemies(AttackSettings settings)
    {
        // Place the hitbox in front of the attacking player.
        float direction = facingRight ? 1f : -1f;

        Vector2 hitboxCenter =
            (Vector2)transform.position +
            new Vector2(direction * settings.range, 0f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            hitboxCenter,
            settings.hitboxSize,
            0f
        );

        // Prevent hitting the same player more than once
        // when they have multiple colliders.
        HashSet<Combat> hitPlayers = new HashSet<Combat>();

        foreach (Collider2D hit in hits)
        {
            Combat targetCombat =
                hit.GetComponentInParent<Combat>();

            // Ignore the attacker and objects that aren't players.
            if (targetCombat == null || targetCombat == this)
                continue;

            if (!hitPlayers.Add(targetCombat))
                continue;

            // Apply damage.
            Damage damageScript =
                targetCombat.GetComponent<Damage>();

            if (damageScript == null)
                damageScript =
                    targetCombat.GetComponentInChildren<Damage>();

            if (damageScript == null)
                damageScript =
                    targetCombat.GetComponentInParent<Damage>();

            if (damageScript != null)
            {
                damageScript.TakeDamage(settings.damage);
            }
            else
            {
                Debug.LogWarning(
                    targetCombat.name + " is missing a Damage component."
                );
            }

            // Apply knockback.
            Knockback knockback =
                targetCombat.GetComponent<Knockback>();

            if (knockback == null)
                knockback =
                    targetCombat.GetComponentInChildren<Knockback>();

            if (knockback == null)
                knockback =
                    targetCombat.GetComponentInParent<Knockback>();

            if (knockback != null)
            {
                knockback.ApplyKnockback(
                    settings.horizontalKnockback,
                    settings.verticalKnockback,
                    facingRight
                );
            }

            // Apply hit stun.
            targetCombat.ApplyHitStun(
                targetCombat.hitStunDuration
            );

            Debug.Log(
                targetCombat.name +
                " was hit! Damage, knockback, and hit stun applied."
            );
        }
    }

    public void ApplyHitStun(float duration)
    {
        // Restart the stun timer if hit again.
        if (hitStunCoroutine != null)
        {
            StopCoroutine(hitStunCoroutine);
        }

        hitStunCoroutine = StartCoroutine(
            HitStunRoutine(duration)
        );
    }

    private IEnumerator HitStunRoutine(float duration)
    {
        isStunned = true;

        // Remember whether movement was enabled before stun.
        if (movementScript != null)
        {
            movementWasEnabled = movementScript.enabled;
            movementScript.enabled = false;
        }
        else
        {
            Debug.LogWarning(
                name + " has no Movement Script assigned for hit stun."
            );
        }

        Debug.Log(name + " is in hit stun!");

        yield return new WaitForSeconds(duration);

        // Restore movement to its previous state.
        if (movementScript != null)
        {
            movementScript.enabled = movementWasEnabled;
        }

        isStunned = false;
        hitStunCoroutine = null;

        Debug.Log(name + " recovered from hit stun!");
    }

    private void OnDrawGizmosSelected()
    {
        if (attackSettings == null)
            return;

        Gizmos.color = Color.red;

        foreach (AttackSettings settings in attackSettings)
        {
            if (settings == null)
                continue;

            float direction = facingRight ? 1f : -1f;

            Vector2 center =
                (Vector2)transform.position +
                new Vector2(direction * settings.range, 0f);

            Gizmos.DrawWireCube(center, settings.hitboxSize);
        }
    }
}