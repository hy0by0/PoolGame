using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class PlayerMovement2D : MonoBehaviour
{
    [Header("Inspector References")]
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private Collider2D bodyCollider;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float groundCheckDistance = 0.06f;

    [Header("Ground Movement")]
    [SerializeField] private float groundMoveSpeed = 5.5f;
    [SerializeField] private float airMoveSpeed = 4.2f;
    [SerializeField] private float acceleration = 45f;

    [Header("Jump")]
    [SerializeField] private bool jumpEnabled = true;
    [SerializeField] private float jumpSpeed = 8.5f;
    [SerializeField] private float jumpBufferTime = 0.12f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpCutGravityMultiplier = 2.1f;
    [SerializeField] private float fallGravityMultiplier = 1.8f;

    [Header("Water Movement")]
    [SerializeField] private LayerMask waterLayerMask;
    [SerializeField] private float waterHorizontalSpeed = 3.2f;
    [SerializeField] private float buoyancyAcceleration = 28f; //浮力加速度
    [SerializeField] private float maxWaterRiseSpeed = 3.6f;
    [SerializeField] private float waterSurfaceOffset = 0.08f;
    [SerializeField] private float waterActivationDepth = 0.08f;

    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];
    private ContactFilter2D groundContactFilter;
    private Vector2 moveInput;
    private WaterVolume2D currentWater;
    private int waterContactCount;
    private float jumpBufferTimer;
    private float coyoteTimer;
    private bool grounded;
    private bool jumpHeld;
    private bool waterMovementActive;

    private void Awake()
    {
        groundContactFilter = new ContactFilter2D();
        groundContactFilter.SetLayerMask(groundLayerMask);
        groundContactFilter.useTriggers = false;
    }

    private void FixedUpdate()
    {
        UpdateGroundedState();
        UpdateJumpTimers();

        if (waterContactCount > 0)
        {
            UpdateWaterMovement();
        }
        else
        {
            waterMovementActive = false;
            UpdateGroundAndAirMovement();
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        jumpHeld = value.isPressed;

        if (value.isPressed)
        {
            jumpBufferTimer = jumpBufferTime;
        }
    }

    private void UpdateGroundedState()
    {
        grounded = bodyCollider.Cast(Vector2.down, groundContactFilter, groundHits, groundCheckDistance) > 0;
    }

    private void UpdateJumpTimers()
    {
        jumpBufferTimer -= Time.fixedDeltaTime;

        if (grounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.fixedDeltaTime;
        }
    }

    private void UpdateGroundAndAirMovement()
    {
        Vector2 velocity = playerRigidbody.linearVelocity;
        float moveSpeed = grounded ? groundMoveSpeed : airMoveSpeed;
        velocity.x = Mathf.MoveTowards(velocity.x, moveInput.x * moveSpeed, acceleration * Time.fixedDeltaTime);

        if (jumpEnabled && jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            velocity.y = jumpSpeed;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        if (!jumpHeld && velocity.y > 0f)
        {
            velocity.y += Physics2D.gravity.y * (jumpCutGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (velocity.y < 0f)
        {
            velocity.y += Physics2D.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }

        playerRigidbody.linearVelocity = velocity;
    }

    private void UpdateWaterMovement()
    {
        Vector2 velocity = playerRigidbody.linearVelocity;
        Bounds playerBounds = bodyCollider.bounds;
        float waterSurfaceY = currentWater.SurfaceY;
        bool fullyInsideWater = playerBounds.max.y <= waterSurfaceY - waterActivationDepth;

        if (fullyInsideWater)
        {
            waterMovementActive = true;
        }

        if (waterMovementActive)
        {
            velocity.x = Mathf.MoveTowards(velocity.x, moveInput.x * waterHorizontalSpeed, acceleration * Time.fixedDeltaTime);

            float waterSurfaceBodyCenterY = waterSurfaceY - playerBounds.extents.y - waterSurfaceOffset;
            bool reachedSurface = playerRigidbody.position.y >= waterSurfaceBodyCenterY;

            if (reachedSurface)
            {
                Vector2 surfacePosition = playerRigidbody.position;
                surfacePosition.y = waterSurfaceBodyCenterY;
                playerRigidbody.position = surfacePosition;
                velocity.y = Mathf.Min(velocity.y, 0f);
            }
            else
            {
                velocity.y = Mathf.MoveTowards(velocity.y, maxWaterRiseSpeed, buoyancyAcceleration * Time.fixedDeltaTime);
            }
        }
        else
        {
            float moveSpeed = grounded ? groundMoveSpeed : airMoveSpeed;
            velocity.x = Mathf.MoveTowards(velocity.x, moveInput.x * moveSpeed, acceleration * Time.fixedDeltaTime);
        }

        playerRigidbody.linearVelocity = velocity;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        int otherLayerBit = 1 << other.gameObject.layer;

        if ((waterLayerMask.value & otherLayerBit) != 0)
        {
            waterContactCount++;
            currentWater = other.GetComponent<WaterVolume2D>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        int otherLayerBit = 1 << other.gameObject.layer;

        if ((waterLayerMask.value & otherLayerBit) != 0)
        {
            waterContactCount = Mathf.Max(0, waterContactCount - 1);

            if (waterContactCount == 0)
            {
                waterMovementActive = false;
            }
        }
    }

    private void Reset()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
    }
}
