using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private Transform facingPivot;

    [SerializeField] private Animator _animator;
                     private SpriteRenderer _spriteRenderer;


    // =========================================================
    // GROUND MOVEMENT
    // =========================================================
    [Header("Ground Movement")]

    public float maxRunSpeed = 8f;
    public float groundAcceleration = 60f;
    public float groundDeceleration = 70f;


    // =========================================================
    // AIR MOVEMENT
    // =========================================================
    [Header("Air Movement")]

    public float maxAirSpeed = 7f;
    public float airAcceleration = 35f;
    public float airDeceleration = 20f;


    // =========================================================
    // JUMP
    // =========================================================
    [Header("Jump")]

    public float jumpForce = 14f;

    // 2 = normal jump + double jump.
    public int maxJumps = 2;

    // Controls how much the jump is shortened when
    // the player releases the jump button early.
    public float jumpCutMultiplier = 0.5f;


    // =========================================================
    // JUMP ASSISTANCE
    // =========================================================
    [Header("Jump Assistance")]

    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;


    // =========================================================
    // FAST FALL
    // =========================================================
    [Header("Fast Fall")]

    public float fastFallSpeed = 18f;


    // =========================================================
    // GROUND CHECK
    // =========================================================
    [Header("Ground Check")]

    // The GroundCheck object underneath Player 1.
    public Transform groundCheck;

    // Size of the ground detection circle.
    public float groundCheckRadius = 0.15f;

    // Only objects on this layer count as ground.
    public LayerMask groundLayer;


    // =========================================================
    // PRIVATE VARIABLES
    // =========================================================

    private Rigidbody2D rb;

    private float horizontalInput;

    // Number of jumps Player 1 has remaining.
    private int jumpsRemaining;

    // Is Player 1 currently touching the ground?
    private bool isGrounded;

    // Was Player 1 grounded during the previous check?
    private bool wasGrounded;

    // Coyote time countdown.
    private float coyoteTimer;

    // Jump buffer countdown.
    private float jumpBufferTimer;


    // =========================================================
    // START
    // =========================================================
    void Start()
    {
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


    // =========================================================
    // UPDATE
    // =========================================================
    void Update()
    {
        GetInput();

        CheckGround();

        HandleJump();

        HandleFastFall();
    }


    // =========================================================
    // FIXED UPDATE
    // =========================================================
    void FixedUpdate()
    {
        HandleHorizontalMovement();
    }


    // =========================================================
    // GET INPUT
    // =========================================================
    void GetInput()
    {
        horizontalInput = 0f;
        
        // A = move left.
        if (Keyboard.current.aKey.isPressed)
        {
            horizontalInput = -1f;
            facingPivot.localScale = new Vector3(-1f, 1f, 1f);
            _animator.SetBool("IsWalking", true);
        }

        // D = move right.
        if (Keyboard.current.dKey.isPressed)
        {
            horizontalInput = 1f;
            facingPivot.localScale = new Vector3(1f, 1f, 1f);
            _animator.SetBool("IsWalking", true);
        }

        if (!Keyboard.current.aKey.isPressed && !Keyboard.current.dKey.isPressed)
        {
            _animator.SetBool("IsWalking", false);
        }

    }


    // =========================================================
    // CHECK GROUND
    // =========================================================
    void CheckGround()
    {
        // Remember the previous grounded state.
        wasGrounded = isGrounded;

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

        // -----------------------------------------------------
        // COYOTE TIME
        // -----------------------------------------------------
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


    // =========================================================
    // HORIZONTAL MOVEMENT
    // =========================================================
    void HandleHorizontalMovement()
    {
        float targetSpeed;
        float acceleration;

        // -----------------------------------------------------
        // GROUND MOVEMENT
        // -----------------------------------------------------
        if (isGrounded)
        {
            targetSpeed = horizontalInput * maxRunSpeed;

            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                acceleration = groundAcceleration;
            }
            else
            {
                acceleration = groundDeceleration;
            }
        }

        // -----------------------------------------------------
        // AIR MOVEMENT
        // -----------------------------------------------------
        else
        {
            targetSpeed = horizontalInput * maxAirSpeed;

            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                acceleration = airAcceleration;
            }
            else
            {
                acceleration = airDeceleration;
            }
        }

        // Change horizontal speed without changing vertical speed.
        rb.linearVelocity = new Vector2(
            Mathf.MoveTowards(
                rb.linearVelocity.x,
                targetSpeed,
                acceleration * Time.fixedDeltaTime
            ),
            rb.linearVelocity.y
        );
    }


    // =========================================================
    // JUMP
    // =========================================================
    void HandleJump()
    {
        // -----------------------------------------------------
        // JUMP BUFFER
        // -----------------------------------------------------
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpBufferTimer = jumpBufferTime;

        }

        if (jumpBufferTimer > 0)
        {
            jumpBufferTimer -= Time.deltaTime;
        }


        // -----------------------------------------------------
        // FIRST JUMP
        // -----------------------------------------------------
        if (jumpBufferTimer > 0 &&
            (isGrounded || coyoteTimer > 0) &&
            jumpsRemaining > 0)
        {
            PerformJump();
            _animator.SetTrigger("JumpStart");
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }


        // -----------------------------------------------------
        // DOUBLE JUMP
        // -----------------------------------------------------
        else if (Keyboard.current.spaceKey.wasPressedThisFrame &&
                 !isGrounded &&
                 coyoteTimer <= 0 &&
                 jumpsRemaining > 0)
        {
            PerformJump();
            _animator.SetTrigger("DoubleJump");
        }


        // -----------------------------------------------------
        // VARIABLE JUMP HEIGHT
        // -----------------------------------------------------
        if (Keyboard.current.spaceKey.wasReleasedThisFrame &&
            rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * jumpCutMultiplier
            );
        }
    }


    // =========================================================
    // PERFORM JUMP
    // =========================================================
    void PerformJump()
    {
        // Give Player 1 upward velocity.
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        // Use one jump.
        jumpsRemaining--;
    }


    // =========================================================
    // FAST FALL
    // =========================================================
    void HandleFastFall()
    {
        // S makes Player 1 fall faster.
        if (!isGrounded &&
            Keyboard.current.sKey.isPressed &&
            rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                -fastFallSpeed
            );
        }
    }


    // =========================================================
    // GROUND CHECK VISUALIZATION
    // =========================================================
    void OnDrawGizmosSelected()
    {
        // Draw the GroundCheck circle in the Scene view.
        if (groundCheck != null)
        {
            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }
    }
}