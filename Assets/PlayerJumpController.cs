using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerJumpController : MonoBehaviour
{
    [HideInInspector]
    public float jumpPower = 500f;

    [Header("Jump Feel")]
    [SerializeField] float tunedJumpVelocity = 9f;
    [SerializeField] float tunedEstimatedJumpHeight = 1.7f;
    [SerializeField] [Range(0.1f, 0.95f)] float tunedFastRiseHeightRatio = 0.75f;
    [SerializeField] float tunedFastRiseGravityScale = 1.85f;
    [SerializeField] float tunedApexGravityScale = 2.6f;
    [SerializeField] float tunedFallGravityScale = 3.6f;
    [SerializeField] float tunedFastFallGravityScale = 7.5f;
    [SerializeField] float tunedFastFallStartVelocity = -0.35f;
    [SerializeField] float tunedApexVelocityThreshold = 0.25f;
    [SerializeField] float tunedMaxFallSpeed = 18f;

    [Header("Jump Sprite")]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Animator spriteAnimator;
    [SerializeField] bool useAnimatorWhileGrounded = true;
    [SerializeField] Sprite normalSprite;
    [SerializeField] Sprite jumpSprite;

    Rigidbody2D rb2d;
    Collider2D playerCollider;
    float defaultGravityScale = 1f;
    float jumpStartY;
    bool isJumping;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        defaultGravityScale = rb2d.gravityScale;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteAnimator == null)
        {
            spriteAnimator = GetComponent<Animator>();
        }

        if (spriteRenderer != null && normalSprite == null)
        {
            normalSprite = spriteRenderer.sprite;
        }
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        bool grounded = IsGrounded();

        if (keyboard == null)
        {
            UpdateJumpSprite(grounded);
            return;
        }

        if (keyboard.spaceKey.wasPressedThisFrame && grounded)
        {
            Jump();
            grounded = false;
        }

        UpdateJumpSprite(grounded);
    }

    void FixedUpdate()
    {
        UpdateJumpGravity(IsGrounded());
    }

    void Jump()
    {
        Vector2 velocity = rb2d.linearVelocity;
        velocity.y = tunedJumpVelocity;
        rb2d.linearVelocity = velocity;
        jumpStartY = transform.position.y;
        isJumping = true;
        rb2d.gravityScale = tunedFastRiseGravityScale;
    }

    void UpdateJumpGravity(bool grounded)
    {
        if (rb2d == null)
        {
            return;
        }

        if (grounded && rb2d.linearVelocity.y <= 0.05f)
        {
            isJumping = false;
            rb2d.gravityScale = defaultGravityScale;
            return;
        }

        float verticalVelocity = rb2d.linearVelocity.y;

        if (verticalVelocity > tunedApexVelocityThreshold)
        {
            float heightProgress = Mathf.Clamp01((transform.position.y - jumpStartY) / tunedEstimatedJumpHeight);
            rb2d.gravityScale = heightProgress < tunedFastRiseHeightRatio ? tunedFastRiseGravityScale : tunedApexGravityScale;
        }
        else if (isJumping && Mathf.Abs(verticalVelocity) <= tunedApexVelocityThreshold)
        {
            rb2d.gravityScale = tunedApexGravityScale;
        }
        else if (verticalVelocity < -Mathf.Abs(tunedApexVelocityThreshold))
        {
            rb2d.gravityScale = verticalVelocity <= tunedFastFallStartVelocity ? tunedFastFallGravityScale : tunedFallGravityScale;
        }

        if (tunedMaxFallSpeed > 0f && rb2d.linearVelocity.y < -tunedMaxFallSpeed)
        {
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, -tunedMaxFallSpeed);
        }
    }

    bool IsGrounded()
    {
        Bounds bounds = playerCollider.bounds;
        Vector2 checkPosition = new Vector2(bounds.center.x, bounds.min.y - 0.05f);
        Vector2 checkSize = new Vector2(bounds.size.x * 0.8f, 0.1f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(checkPosition, checkSize, 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit != playerCollider && !hit.isTrigger)
            {
                return true;
            }
        }

        return false;
    }

    void UpdateJumpSprite(bool grounded)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (spriteAnimator != null)
        {
            spriteAnimator.enabled = grounded && useAnimatorWhileGrounded;

            if (spriteAnimator.enabled)
            {
                return;
            }
        }

        Sprite nextSprite = grounded ? normalSprite : jumpSprite;

        if (nextSprite != null && spriteRenderer.sprite != nextSprite)
        {
            spriteRenderer.sprite = nextSprite;
        }
    }

    void OnDisable()
    {
        if (rb2d != null)
        {
            rb2d.gravityScale = defaultGravityScale;
        }
    }

    void OnValidate()
    {
        jumpPower = Mathf.Max(0f, jumpPower);
        tunedJumpVelocity = Mathf.Max(0.01f, tunedJumpVelocity);
        tunedEstimatedJumpHeight = Mathf.Max(0.01f, tunedEstimatedJumpHeight);
        tunedFastRiseGravityScale = Mathf.Max(0.01f, tunedFastRiseGravityScale);
        tunedApexGravityScale = Mathf.Max(0.01f, tunedApexGravityScale);
        tunedFallGravityScale = Mathf.Max(0.01f, tunedFallGravityScale);
        tunedFastFallGravityScale = Mathf.Max(tunedFallGravityScale, tunedFastFallGravityScale);
        tunedFastFallStartVelocity = -Mathf.Abs(tunedFastFallStartVelocity);
        tunedApexVelocityThreshold = Mathf.Max(0.01f, tunedApexVelocityThreshold);
        tunedMaxFallSpeed = Mathf.Max(0f, tunedMaxFallSpeed);
    }
}
