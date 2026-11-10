using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform facingPivot;
    [SerializeField] private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    // =========================================================
    // INPUT SYSTEM SETUP
    // =========================================================
    [Header("Input Configuration")]
    [SerializeField] private PlayerInput playerInput; 
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction crouchAction;

    [Header("Ground Movement")]
    public float maxRunSpeed = 8f;
    public float groundAcceleration = 60f;
    public float groundDeceleration = 70f;

    [Header("Crouch Movement")]
    public float crouchSpeedMultiplier = 0.5f;
    [SerializeField] private Collider2D standingCollider;
    [SerializeField] private Collider2D crouchingCollider;

    [Header("Air Movement")]
    public float maxAirSpeed = 7f;
    public float airAcceleration = 35f;
    public float airDeceleration = 20f;

    [Header("Jump")]
    public float jumpForce = 14f;
    public int maxJumps = 2; 
    public float jumpCutMultiplier = 0.5f;

    [Header("Jump Assistance")]
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    [Header("Fast Fall")]
    public float fastFallSpeed = 18f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float horizontalInput;
    private int jumpsRemaining;
    private bool isGrounded;
    private bool wasGrounded;
    private float coyoteTimer;
    private float jumpBufferTimer;

    private bool isCrouching;
    private bool isStunned;
    private float stunTimer;

    void Awake()
    {
        // Cache your references from the PlayerInput Component
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        
        // Match the exact string names you created in your Input Action Asset
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        crouchAction = playerInput.actions["Crouch"];
        // Get Player 1's Rigidbody2D.
        rb = GetComponent<Rigidbody2D>();

        _animator = GetComponentInChildren<Animator>();

        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Give Player 1 their starting jumps.
        jumpsRemaining = maxJumps;

        // Start with ground detection turned off until
        // the first ground check happens.
        isGrounded = false;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        jumpsRemaining = maxJumps;
        isGrounded = false;
        SetColliderState(false); 
    }

    void Update()
    {
        CheckGround();

        if (isStunned)
        {
            HandleStunTimers();
            return; 
        }

        GetInput();
        HandleCrouch();
        HandleJump();
        HandleFastFall();
    }

    void FixedUpdate()
    {
        if (isStunned) return; 
        HandleHorizontalMovement();
    }

    void GetInput()
    {
        // Automatically reads from keyboard WASD OR Gamepad D-pad/Joystick
        Vector2 movementVector = moveAction.ReadValue<Vector2>();
        horizontalInput = movementVector.x;

        if (Mathf.Abs(horizontalInput) > 0.05f)
        {
            float direction = Mathf.Sign(horizontalInput);
            facingPivot.localScale = new Vector3(direction, 1f, 1f);
            _animator.SetBool("IsWalking", true);
        }
        else
        {
            _animator.SetBool("IsWalking", false);
        }
    }

    void CheckGround()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Check whether GroundCheck is touching the Ground layer.
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );


        // Grounded check for jump animation
        _animator.SetBool("IsGrounded", isGrounded);


        // -----------------------------------------------------
        // LANDING
        // -----------------------------------------------------
        // Only give the jumps back when the player actually
        // goes from being in the air to being on the ground.
        // -----------------------------------------------------
        if (!wasGrounded && isGrounded)
        {
            jumpsRemaining = maxJumps;
        }

        if (wasGrounded && !isGrounded)
        {
            coyoteTimer = coyoteTime;
        }

        if (!isGrounded)
        {
            coyoteTimer -= Time.deltaTime;
        }
        else
        {
            coyoteTimer = coyoteTime;
        }
    }

    void HandleHorizontalMovement()
    {
        float targetSpeed;
        float acceleration;

        if (isGrounded)
        {
            float currentMaxSpeed = isCrouching ? maxRunSpeed * crouchSpeedMultiplier : maxRunSpeed;
            targetSpeed = horizontalInput * currentMaxSpeed;
            acceleration = Mathf.Abs(horizontalInput) > 0.01f ? groundAcceleration : groundDeceleration;
        }
        else
        {
            targetSpeed = horizontalInput * maxAirSpeed;
            acceleration = Mathf.Abs(horizontalInput) > 0.01f ? airAcceleration : airDeceleration;
        }

        rb.linearVelocity = new Vector2(
            Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime),
            rb.linearVelocity.y
        );
    }

    void HandleCrouch()
    {
        // Detects if crouch binding is held down (Keyboard S or Gamepad button)
        if (crouchAction.IsPressed() && isGrounded)
        {
            if (!isCrouching)
            {
                isCrouching = true;
                _animator.SetBool("IsCrouching", true);
                SetColliderState(true);
            }
        }
        else
        {
            if (isCrouching)
            {
                isCrouching = false;
                _animator.SetBool("IsCrouching", false);
                SetColliderState(false);
            }
        }
    }

    private void SetColliderState(bool crouching)
    {
        if (standingCollider != null && crouchingCollider != null)
        {
            standingCollider.enabled = !crouching;
            crouchingCollider.enabled = crouching;
        }
    }

    void HandleJump()
    {
        if (isCrouching) return;

        if (jumpAction.WasPressedThisFrame())
        {
            jumpBufferTimer = jumpBufferTime;

        }

        if (jumpBufferTimer > 0)
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (jumpBufferTimer > 0 && (isGrounded || coyoteTimer > 0) && jumpsRemaining > 0)
        {
            PerformJump();
            _animator.SetTrigger("JumpStart");
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }
        else if (jumpAction.WasPressedThisFrame() && !isGrounded && coyoteTimer <= 0 && jumpsRemaining > 0)
        {
            PerformJump();
            _animator.SetTrigger("DoubleJump");
        }

        if (jumpAction.WasReleasedThisFrame() && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpsRemaining--;
    }

    void HandleFastFall()
    {
        // Vector2 down check for the stick configuration or down key map
        Vector2 movementVector = moveAction.ReadValue<Vector2>();
        bool pullingDown = movementVector.y < -0.5f || crouchAction.IsPressed();

        if (!isGrounded && pullingDown && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -fastFallSpeed);
        }
    }

    public void ApplyKnockback(Vector2 force, float stunDuration)
    {
        isStunned = true;
        stunTimer = stunDuration;

        if (isCrouching)
        {
            isCrouching = false;
            _animator.SetBool("IsCrouching", false);
            SetColliderState(false);
        }

        _animator.SetBool("IsWalking", false);
        _animator.SetTrigger("HitStun");
        rb.linearVelocity = force;
    }

    private void HandleStunTimers()
    {
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0f)
        {
            isStunned = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}