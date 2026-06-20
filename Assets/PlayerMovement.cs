using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] bool lockAirTurnaround = true;
    [SerializeField] float groundedCheckDistance = 0.08f;
    [SerializeField] float paperFlipDuration = 0.18f;
    [SerializeField] float paperFlipThinScale = 0.08f;
    [SerializeField] float paperFlipTilt = 4f;

    Collider2D playerCollider;
    SpriteRenderer flipRenderer;
    Coroutine flipCoroutine;
    bool isFacingLeft;
    int airMoveDirection;
    bool wasGrounded;

    void Awake()
    {
        playerCollider = GetComponent<Collider2D>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            isFacingLeft = spriteRenderer.flipX;
            CreateFlipRenderer();
        }
    }

    void Update()
    {
        float x = 0f;
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            x = -1f;
        }

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            x = 1f;
        }

        x = ApplyAirTurnLock(x);
        UpdateFacingDirection(x);
        transform.position += new Vector3(x * speed * Time.deltaTime, 0, 0);
    }

    float ApplyAirTurnLock(float inputX)
    {
        if (!lockAirTurnaround)
        {
            return inputX;
        }

        bool grounded = IsGrounded();

        if (grounded)
        {
            wasGrounded = true;
            airMoveDirection = 0;
            return inputX;
        }

        if (wasGrounded)
        {
            airMoveDirection = GetDirectionSign(inputX);

            if (airMoveDirection == 0)
            {
                airMoveDirection = isFacingLeft ? -1 : 1;
            }

            wasGrounded = false;
        }

        int inputDirection = GetDirectionSign(inputX);

        if (inputDirection == 0)
        {
            return 0f;
        }

        return inputDirection == airMoveDirection ? inputX : 0f;
    }

    bool IsGrounded()
    {
        if (playerCollider == null)
        {
            return true;
        }

        Bounds bounds = playerCollider.bounds;
        Vector2 checkPosition = new Vector2(bounds.center.x, bounds.min.y - groundedCheckDistance * 0.5f);
        Vector2 checkSize = new Vector2(bounds.size.x * 0.8f, groundedCheckDistance);
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

    int GetDirectionSign(float x)
    {
        if (x > 0f)
        {
            return 1;
        }

        if (x < 0f)
        {
            return -1;
        }

        return 0;
    }

    void OnDisable()
    {
        if (flipCoroutine != null)
        {
            StopCoroutine(flipCoroutine);
            flipCoroutine = null;
        }

        if (flipRenderer != null)
        {
            flipRenderer.enabled = false;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.flipX = isFacingLeft;
        }
    }

    void UpdateFacingDirection(float x)
    {
        if (spriteRenderer == null || Mathf.Approximately(x, 0f))
        {
            return;
        }

        bool shouldFaceLeft = x < 0f;
        if (shouldFaceLeft == isFacingLeft)
        {
            return;
        }

        if (paperFlipDuration <= 0f || flipRenderer == null)
        {
            isFacingLeft = shouldFaceLeft;
            spriteRenderer.flipX = shouldFaceLeft;
            return;
        }

        if (flipCoroutine != null)
        {
            StopCoroutine(flipCoroutine);
        }

        flipCoroutine = StartCoroutine(AnimatePaperFlip(shouldFaceLeft));
    }

    void CreateFlipRenderer()
    {
        GameObject flipObject = new GameObject("PaperFlipVisual");
        flipObject.transform.SetParent(transform, false);

        flipRenderer = flipObject.AddComponent<SpriteRenderer>();
        flipRenderer.enabled = false;
    }

    IEnumerator AnimatePaperFlip(bool faceLeft)
    {
        bool startFacingLeft = isFacingLeft;
        isFacingLeft = faceLeft;

        CopyRendererState(spriteRenderer, flipRenderer);
        flipRenderer.flipX = startFacingLeft;
        flipRenderer.enabled = true;
        spriteRenderer.enabled = false;

        float halfDuration = paperFlipDuration * 0.5f;
        float timer = 0f;

        while (timer < halfDuration)
        {
            float t = Mathf.Clamp01(timer / halfDuration);
            SetFlipVisual(t, 1f, paperFlipThinScale, startFacingLeft);
            timer += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.flipX = faceLeft;
        flipRenderer.flipX = faceLeft;
        timer = 0f;

        while (timer < halfDuration)
        {
            float t = Mathf.Clamp01(timer / halfDuration);
            SetFlipVisual(t, paperFlipThinScale, 1f, faceLeft);
            timer += Time.deltaTime;
            yield return null;
        }

        flipRenderer.enabled = false;
        spriteRenderer.enabled = true;
        spriteRenderer.flipX = faceLeft;
        flipCoroutine = null;
    }

    void SetFlipVisual(float t, float fromScaleX, float toScaleX, bool facingLeft)
    {
        CopyRendererState(spriteRenderer, flipRenderer);
        float eased = 1f - Mathf.Pow(1f - t, 2f);
        float scaleX = Mathf.Lerp(fromScaleX, toScaleX, eased);
        float direction = facingLeft ? -1f : 1f;

        flipRenderer.transform.localScale = new Vector3(scaleX, 1f + (1f - scaleX) * 0.04f, 1f);
        flipRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, direction * paperFlipTilt * Mathf.Sin(t * Mathf.PI));
    }

    void CopyRendererState(SpriteRenderer source, SpriteRenderer target)
    {
        if (source == null || target == null)
        {
            return;
        }

        target.sprite = source.sprite;
        target.color = source.color;
        target.sortingLayerID = source.sortingLayerID;
        target.sortingOrder = source.sortingOrder;
        target.material = source.sharedMaterial;
        target.flipY = source.flipY;
    }

    void OnValidate()
    {
        speed = Mathf.Max(0f, speed);
        groundedCheckDistance = Mathf.Max(0.01f, groundedCheckDistance);
        paperFlipDuration = Mathf.Max(0f, paperFlipDuration);
        paperFlipThinScale = Mathf.Clamp(paperFlipThinScale, 0.01f, 1f);
        paperFlipTilt = Mathf.Max(0f, paperFlipTilt);
    }
}
