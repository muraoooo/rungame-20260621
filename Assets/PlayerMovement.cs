using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float paperFlipDuration = 0.18f;
    [SerializeField] float paperFlipThinScale = 0.08f;
    [SerializeField] float paperFlipTilt = 4f;

    SpriteRenderer flipRenderer;
    Coroutine flipCoroutine;
    bool isFacingLeft;

    void Awake()
    {
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

        UpdateFacingDirection(x);
        transform.position += new Vector3(x * speed * Time.deltaTime, 0, 0);
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
}
