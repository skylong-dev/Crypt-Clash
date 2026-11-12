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
    [SerializeField] private Transform facingPivot;

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
    private float hitStunEndsAt;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction lightAttackAction;
    private InputAction heavyAttackAction;
    private Damage damageReceiver;
    private Knockback knockbackReceiver;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (facingPivot == null)
            facingPivot = transform.Find("FacingPivot");

        var movement = GetComponent<PlayerMovement>();
        if (movementScript == null)
            movementScript = movement;

        if (groundCheck == null && movement != null)
        {
            groundCheck = movement.groundCheck;
            groundCheckRadius = movement.groundCheckRadius;
            groundLayer = movement.groundLayer;
        }

        damageReceiver = FindFighterComponent<Damage>();
        knockbackReceiver = FindFighterComponent<Knockback>();
    }

    private T FindFighterComponent<T>() where T : Component
    {
        T component = GetComponent<T>();
        if (component == null)
            component = GetComponentInChildren<T>();

        return component != null ? component : GetComponentInParent<T>();
    }

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        var actions = playerInput != null ? playerInput.actions : null;
        if (actions == null)
        {
            Debug.LogWarning("Combat needs the fighter's Player Input and Controls asset.", this);
            return;
        }

        moveAction = actions.FindAction("Player/Move");
        lightAttackAction = actions.FindAction("Player/LightAttack");
        heavyAttackAction = actions.FindAction("Player/HeavyAttack");

        if (moveAction == null || lightAttackAction == null || heavyAttackAction == null)
        {
            Debug.LogWarning(
                "Combat expects Move, LightAttack, and HeavyAttack in the Player action map.",
                this
            );
        }
    }

    private void LateUpdate()
    {
        if (isStunned)
        {
            if (Time.time < hitStunEndsAt)
                return;

            ClearHitStun();
        }

        if (playerInput == null || !playerInput.inputIsActive ||
            moveAction == null || !moveAction.enabled || Time.time < nextAttackTime ||
            (damageReceiver != null && damageReceiver.IsDefeated()))
        {
            return;
        }

        if (lightAttackAction != null && lightAttackAction.enabled && lightAttackAction.WasPressedThisFrame())
            PerformAttack(AttackType.Light);
        else if (heavyAttackAction != null && heavyAttackAction.enabled && heavyAttackAction.WasPressedThisFrame())
            PerformAttack(AttackType.Heavy);
    }

    private void ReadDirectionInput()
    {
        Vector2 direction = moveAction.ReadValue<Vector2>();

        var keyboard = playerInput.GetDevice<Keyboard>();
        if (keyboard != null)
        {
            bool left = direction.x < 0f || keyboard.leftArrowKey.isPressed;
            bool right = direction.x > 0f || keyboard.rightArrowKey.isPressed;
            bool down = direction.y < 0f || keyboard.downArrowKey.isPressed;
            bool up = direction.y > 0f || keyboard.upArrowKey.isPressed;
            direction.x = (right ? 1f : 0f) - (left ? 1f : 0f);
            direction.y = (up ? 1f : 0f) - (down ? 1f : 0f);
        }

        var gamepad = playerInput.GetDevice<Gamepad>();
        if (gamepad != null && direction.sqrMagnitude <= 0.25f * 0.25f)
            direction = gamepad.dpad.ReadValue();

        horizontalInput = Mathf.Abs(direction.x) > 0.25f ? Mathf.Sign(direction.x) : 0f;
        verticalInput = Mathf.Abs(direction.y) > 0.25f ? Mathf.Sign(direction.y) : 0f;
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

    private void PerformAttack(AttackType type)
    {
        if (facingPivot != null)
            facingRight = facingPivot.localScale.x >= 0f;

        ReadDirectionInput();
        UpdateGrounded();
        bool airAttack = !isGrounded;
        AttackVariation variation = GetAttackVariation(airAttack);
        AttackSettings settings = FindAttackSettings(type, variation, airAttack);

        if (settings == null)
        {
            Debug.LogWarning($"Missing {type} attack settings for {(airAttack ? "Air" : "Ground")}. Add a Neutral entry.", this);
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        PlayAttackAnimation(type, settings.variation, airAttack);
        CheckForEnemies(settings);
    }

    private AttackSettings FindAttackSettings(AttackType type, AttackVariation variation, bool airAttack)
    {
        if (attackSettings == null)
            return null;

        AttackSettings neutral = null;
        foreach (AttackSettings settings in attackSettings)
        {
            if (settings == null || settings.attackType != type || settings.isAirAttack != airAttack)
                continue;

            if (settings.variation == variation)
                return settings;

            if (neutral == null && settings.variation == AttackVariation.Neutral)
                neutral = settings;
        }

        return neutral;
    }

    private void PlayAttackAnimation(AttackType type, AttackVariation variation, bool airAttack)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        string triggerName = type + (airAttack ? "Air" : "Ground") + variation;
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == triggerName && parameter.type == AnimatorControllerParameterType.Trigger)
            {
                animator.SetTrigger(triggerName);
                return;
            }
        }

        if (type == AttackType.Light)
            Debug.LogWarning("Animator trigger is missing: " + triggerName, this);
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

    private Vector2 GetHitboxDirection(AttackSettings settings)
    {
        Vector2 forward = facingRight ? Vector2.right : Vector2.left;

        switch (settings.variation)
        {
            case AttackVariation.Up:
                return Vector2.up;
            case AttackVariation.Down when settings.isAirAttack:
                return Vector2.down;
            case AttackVariation.Backward:
                return -forward;
            default:
                return forward;
        }
    }

    private void CheckForEnemies(AttackSettings settings)
    {
        Vector2 direction = GetHitboxDirection(settings);
        Vector2 center = (Vector2)transform.position + direction * settings.range;
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, settings.hitboxSize, 0f);
        var hitPlayers = new HashSet<Combat>();

        foreach (Collider2D hit in hits)
        {
            Combat target = hit.GetComponentInParent<Combat>();
            if (target == null || target == this || !hitPlayers.Add(target))
                continue;

            Damage targetDamage = target.damageReceiver;
            if (targetDamage != null)
            {
                if (targetDamage.IsDefeated())
                    continue;

                int hitDamage = Mathf.Max(0, settings.damage);
                bool lethalHit = hitDamage >= targetDamage.GetHealth();
                targetDamage.TakeDamage(hitDamage);

                // The stock manager may respawn the target during TakeDamage.
                if (lethalHit)
                    continue;
            }
            else
            {
                Debug.LogWarning(target.name + " is missing a Damage component.", target);
            }

            target.ApplyHitStun(target.hitStunDuration);

            if (target.knockbackReceiver != null)
            {
                float separation = target.transform.position.x - transform.position.x;
                bool pushRight = Mathf.Approximately(separation, 0f)
                    ? (direction.x == 0f ? facingRight : direction.x > 0f)
                    : separation > 0f;

                target.knockbackReceiver.ApplyKnockback(
                    settings.horizontalKnockback,
                    settings.verticalKnockback,
                    pushRight
                );
            }
            else
            {
                Debug.LogWarning(target.name + " is missing a Knockback component.", target);
            }
        }
    }

    public void ApplyHitStun(float duration)
    {
        if (duration <= 0f || !isActiveAndEnabled)
            return;

        if (!isStunned)
        {
            isStunned = true;
            movementWasEnabled = movementScript != null && movementScript.enabled;
            if (movementWasEnabled)
                movementScript.enabled = false;
        }

        hitStunEndsAt = Mathf.Max(hitStunEndsAt, Time.time + duration);
    }

    public void ClearHitStun()
    {
        if (movementWasEnabled && movementScript != null)
            movementScript.enabled = true;

        movementWasEnabled = false;
        isStunned = false;
        hitStunEndsAt = 0f;
    }

    private void OnDisable()
    {
        ClearHitStun();
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

            Vector2 center = (Vector2)transform.position +
                GetHitboxDirection(settings) * settings.range;
            Gizmos.DrawWireCube(center, settings.hitboxSize);
        }
    }
}
