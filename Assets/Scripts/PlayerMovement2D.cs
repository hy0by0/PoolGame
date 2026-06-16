using UnityEngine;
using UnityEngine.InputSystem;

// プレイヤーの通常移動、ジャンプ、水に入った時の浮上をまとめて扱うクラス。
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
    [SerializeField] private float buoyancyAcceleration = 28f;
    [SerializeField] private float maxWaterRiseSpeed = 3.6f;
    [SerializeField] private float waterSurfaceOffset = 0.35f;
    [SerializeField] private float waterActivationDepth = 0.08f;
    [SerializeField] private float waterHorizontalActivationRatio = 0.7f;
    [SerializeField] private float waterFloatDamping = 6f;
    [SerializeField] private float waterCeilingCheckDistance = 0.04f;

    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];
    private readonly WaterVolume2D[] contactedWaters = new WaterVolume2D[8];
    private ContactFilter2D groundContactFilter;
    private Vector2 moveInput;
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

    // 開始直後から物理演算が眠らないように、状態を初期化してRigidbody2Dを起こす。
    private void OnEnable()
    {
        moveInput = Vector2.zero;
        jumpHeld = false;
        waterMovementActive = false;
        playerRigidbody.WakeUp();
    }

    // 物理更新ごとに、地上/空中/水中のどの移動を行うかを切り替える。
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

    // Input System の Move アクションから移動方向を受け取る。
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Input System の Jump アクションからジャンプ入力を受け取る。
    public void OnJump(InputValue value)
    {
        jumpHeld = value.isPressed;

        if (value.isPressed)
        {
            jumpBufferTimer = jumpBufferTime;
        }
    }

    // 足元に地面があるかを Collider2D.Cast で調べる。
    private void UpdateGroundedState()
    {
        grounded = bodyCollider.Cast(Vector2.down, groundContactFilter, groundHits, groundCheckDistance) > 0;
    }

    // ジャンプ先行入力とコヨーテタイムの残り時間を更新する。
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

    // 水に触れていない時の左右移動とジャンプを処理する。
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

    // 水に完全に入った後、浮力で目標の浮遊高さへ近づける。
    private void UpdateWaterMovement()
    {
        Vector2 velocity = playerRigidbody.linearVelocity;
        Bounds playerBounds = bodyCollider.bounds;
        Bounds waterBounds = FindCurrentWaterBounds();
        float waterSurfaceY = waterBounds.max.y;
        bool fullyInsideWater = IsEnoughInsideWater(playerBounds, waterBounds, waterSurfaceY);

        if (fullyInsideWater)
        {
            waterMovementActive = true;
        }

        if (waterMovementActive)
        {
            velocity.x = Mathf.MoveTowards(velocity.x, moveInput.x * waterHorizontalSpeed, acceleration * Time.fixedDeltaTime);

            float floatingBodyCenterY = waterSurfaceY - waterSurfaceOffset;
            float heightDifference = floatingBodyCenterY - playerRigidbody.position.y;
            float gravityCancelAcceleration = -Physics2D.gravity.y;
            bool blockedAbove = bodyCollider.Cast(Vector2.up, groundContactFilter, groundHits, waterCeilingCheckDistance) > 0;
            bool wantsToRise = heightDifference > 0f;

            if (blockedAbove && wantsToRise)
            {
                velocity.y = Mathf.MoveTowards(velocity.y, 0f, waterFloatDamping * Time.fixedDeltaTime);

                if (velocity.y > 0f)
                {
                    velocity.y = 0f;
                }
            }
            else
            {
                float floatAcceleration = gravityCancelAcceleration + heightDifference * buoyancyAcceleration - velocity.y * waterFloatDamping;
                velocity.y += floatAcceleration * Time.fixedDeltaTime;
                velocity.y = Mathf.Clamp(velocity.y, -maxWaterRiseSpeed, maxWaterRiseSpeed);
            }
        }
        else
        {
            float moveSpeed = grounded ? groundMoveSpeed : airMoveSpeed;
            velocity.x = Mathf.MoveTowards(velocity.x, moveInput.x * moveSpeed, acceleration * Time.fixedDeltaTime);
        }

        playerRigidbody.linearVelocity = velocity;
    }

    // 触れている水の中から、プレイヤーの中心Xに最も合う水領域を選ぶ。
    private Bounds FindCurrentWaterBounds()
    {
        float playerCenterX = playerRigidbody.position.x;
        Bounds selectedBounds = contactedWaters[0].VolumeBounds;
        float selectedScore = GetWaterSelectionScore(selectedBounds, playerCenterX);

        for (int i = 1; i < waterContactCount; i++)
        {
            Bounds waterBounds = contactedWaters[i].VolumeBounds;
            float score = GetWaterSelectionScore(waterBounds, playerCenterX);

            if (score < selectedScore)
            {
                selectedScore = score;
                selectedBounds = waterBounds;
            }
        }

        return selectedBounds;
    }

    // 横から少し触れただけで浮かないように、縦と横の入り込み量を見る。
    private bool IsEnoughInsideWater(Bounds playerBounds, Bounds waterBounds, float waterSurfaceY)
    {
        float horizontalOverlap = GetHorizontalOverlapWidth(playerBounds, waterBounds);
        float requiredOverlap = playerBounds.size.x * waterHorizontalActivationRatio;
        bool enoughHorizontalInside = horizontalOverlap >= requiredOverlap;
        bool enoughVerticalInside = playerBounds.max.y <= waterSurfaceY - waterActivationDepth;
        return enoughHorizontalInside && enoughVerticalInside;
    }

    // プレイヤーと水領域が横方向にどれだけ重なっているかを返す。
    private static float GetHorizontalOverlapWidth(Bounds playerBounds, Bounds waterBounds)
    {
        float overlapMinX = Mathf.Max(playerBounds.min.x, waterBounds.min.x);
        float overlapMaxX = Mathf.Min(playerBounds.max.x, waterBounds.max.x);
        return Mathf.Max(0f, overlapMaxX - overlapMinX);
    }

    // プレイヤー中心が水の横幅内にある水を優先し、同条件なら中心が近い水を選ぶ。
    private static float GetWaterSelectionScore(Bounds waterBounds, float playerCenterX)
    {
        float horizontalDistance = Mathf.Abs(playerCenterX - waterBounds.center.x);
        bool containsPlayerX = playerCenterX >= waterBounds.min.x && playerCenterX <= waterBounds.max.x;
        return containsPlayerX ? horizontalDistance : horizontalDistance + 10000f;
    }

    // 水レイヤーの Trigger に入った時、候補の水領域として保持する。
    private void OnTriggerEnter2D(Collider2D other)
    {
        int otherLayerBit = 1 << other.gameObject.layer;

        if ((waterLayerMask.value & otherLayerBit) != 0)
        {
            if (waterContactCount < contactedWaters.Length)
            {
                contactedWaters[waterContactCount] = other.GetComponent<WaterVolume2D>();
                waterContactCount++;
            }
        }
    }

    // 水レイヤーの Trigger から出た時、保持していた水領域を外す。
    private void OnTriggerExit2D(Collider2D other)
    {
        int otherLayerBit = 1 << other.gameObject.layer;

        if ((waterLayerMask.value & otherLayerBit) != 0)
        {
            WaterVolume2D exitedWater = other.GetComponent<WaterVolume2D>();

            for (int i = 0; i < waterContactCount; i++)
            {
                if (ReferenceEquals(contactedWaters[i], exitedWater))
                {
                    RemoveContactedWaterAt(i);
                    break;
                }
            }

            if (waterContactCount == 0)
            {
                waterMovementActive = false;
            }
        }
    }

    // 配列の途中にある水領域を取り除き、後ろの要素を前へ詰める。
    private void RemoveContactedWaterAt(int removeIndex)
    {
        int lastIndex = waterContactCount - 1;

        for (int i = removeIndex; i < lastIndex; i++)
        {
            contactedWaters[i] = contactedWaters[i + 1];
        }

        waterContactCount = lastIndex;
    }

    // コンポーネント追加直後に、よく使う参照を自動で入れるための補助。
    private void Reset()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
    }
}
