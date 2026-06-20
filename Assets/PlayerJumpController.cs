using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerJumpController : MonoBehaviour
{
    public float jumpPower = 500f;

    [Header("Jump Sprite")]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Animator spriteAnimator;
    [SerializeField] bool useAnimatorWhileGrounded = true;
    [SerializeField] Sprite normalSprite;
    [SerializeField] Sprite jumpSprite;

    Rigidbody2D rb2d;
    Collider2D playerCollider;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

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
            rb2d.AddForce(transform.up * jumpPower);
            grounded = false;
        }

        UpdateJumpSprite(grounded);
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
}
