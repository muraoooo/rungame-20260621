using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] Image gameOverImage;
    [SerializeField] SpriteRenderer gameOverSpriteRenderer;
    [SerializeField] Sprite gameOverSprite;
    [SerializeField] Sprite readyBangSprite;
    [SerializeField] Sprite playerDamageSprite;

    [Header("Game Over")]
    [SerializeField] float dropDuration = 0.22f;
    [SerializeField] float bounceDuration = 1.4f;
    [SerializeField] float bounceHeight = 180f;
    [SerializeField] int bounceCount = 6;
    [SerializeField] float gameOverScreenWidthRatio = 0.85f;
    [SerializeField] float gameOverScreenHeightRatio = 0.45f;

    [Header("Player")]
    [SerializeField] float playerKnockbackDuration = 1.6f;
    [SerializeField] float playerKnockbackHeight = 2.2f;
    [SerializeField] float playerExitViewportPadding = 0.25f;
    [SerializeField] float playerExitScaleMultiplier = 3f;

    [Header("Retry")]
    [SerializeField] float readyBangDuration = 0.8f;
    [SerializeField] float readyBangScreenWidthRatio = 0.75f;
    [SerializeField] float readyBangScreenHeightRatio = 0.3f;

    bool isGameOver;
    bool isRetrying;
    bool isShowingReadyBang;
    bool retryDeclined;
    float gameOverStartedAt;

    public bool IsGameOver => isGameOver;

    void Start()
    {
        SetVisible(gameOverImage, false);
        SetVisible(gameOverSpriteRenderer, false);
    }

    public void TriggerGameOver(GameObject player, Vector2 knockbackDirection)
    {
        if (isGameOver || player == null || !player.CompareTag("Player"))
        {
            return;
        }

        isGameOver = true;
        gameOverStartedAt = Time.unscaledTime;

        ShowGameOverObjects();
        StopPlayer(player);
        StartCoroutine(KnockPlayerOut(player, knockbackDirection));
    }

    void StopPlayer(GameObject player)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        PlayerJumpController jump = player.GetComponent<PlayerJumpController>();
        if (jump != null)
        {
            jump.enabled = false;
        }

        Rigidbody2D rb2d = player.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }
    }

    void ShowGameOverObjects()
    {
        SetVisible(gameOverImage, true);
        SetVisible(gameOverSpriteRenderer, true);

        if (gameOverImage != null && gameOverSprite != null)
        {
            gameOverImage.sprite = gameOverSprite;
        }

        if (gameOverSpriteRenderer != null && gameOverSprite != null)
        {
            gameOverSpriteRenderer.sprite = gameOverSprite;
        }
    }

    IEnumerator KnockPlayerOut(GameObject player, Vector2 knockbackDirection)
    {
        SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
        Animator animator = player.GetComponent<Animator>();
        Rigidbody2D rb2d = player.GetComponent<Rigidbody2D>();
        float originalVisualHeight = spriteRenderer != null ? spriteRenderer.bounds.size.y : 0f;

        if (animator != null)
        {
            animator.enabled = false;
        }

        ShowPlayerDamageSprite(player, spriteRenderer, originalVisualHeight);

        foreach (Collider2D collider2d in player.GetComponents<Collider2D>())
        {
            collider2d.enabled = false;
        }

        if (rb2d != null)
        {
            rb2d.simulated = false;
        }

        float directionX = knockbackDirection.x < 0f ? -1f : 1f;
        Vector3 startPosition = player.transform.position;
        Vector3 endPosition = GetExitPosition(startPosition, directionX);
        Vector3 startScale = player.transform.localScale;
        Vector3 endScale = startScale * playerExitScaleMultiplier;
        float startRotation = player.transform.eulerAngles.z;
        float timer = 0f;

        while (timer < playerKnockbackDuration)
        {
            float t = Mathf.Clamp01(timer / playerKnockbackDuration);
            float yArc = Mathf.Sin(t * Mathf.PI) * playerKnockbackHeight;
            player.transform.position = Vector3.Lerp(startPosition, endPosition, t) + Vector3.up * yArc;
            player.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            player.transform.rotation = Quaternion.Euler(0f, 0f, startRotation + 360f * directionX * t);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        player.transform.position = endPosition;
        player.transform.localScale = endScale;
    }

    void MatchSpriteHeight(SpriteRenderer spriteRenderer, float targetHeight)
    {
        if (spriteRenderer == null || targetHeight <= 0f || spriteRenderer.bounds.size.y <= 0f)
        {
            return;
        }

        float scaleMultiplier = targetHeight / spriteRenderer.bounds.size.y;
        spriteRenderer.transform.localScale *= scaleMultiplier;
    }

    void ShowPlayerDamageSprite(GameObject player, SpriteRenderer spriteRenderer, float originalVisualHeight)
    {
        if (player == null || spriteRenderer == null || playerDamageSprite == null)
        {
            return;
        }

        Vector3 originalVisualCenter = spriteRenderer.bounds.center;
        spriteRenderer.sprite = playerDamageSprite;
        MatchSpriteHeight(spriteRenderer, originalVisualHeight);

        Vector3 centerOffset = originalVisualCenter - spriteRenderer.bounds.center;
        player.transform.position += centerOffset;
    }

    Vector3 GetExitPosition(Vector3 startPosition, float directionX)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null || !mainCamera.orthographic)
        {
            return startPosition + Vector3.right * directionX * 9f;
        }

        float viewportX = directionX > 0f ? 1f + playerExitViewportPadding : -playerExitViewportPadding;
        Vector3 edge = mainCamera.ViewportToWorldPoint(new Vector3(viewportX, 0.5f, Mathf.Abs(mainCamera.transform.position.z - startPosition.z)));
        return new Vector3(edge.x, startPosition.y, startPosition.z);
    }

    void OnGUI()
    {
        if (isShowingReadyBang)
        {
            DrawSprite(readyBangSprite, readyBangScreenWidthRatio, readyBangScreenHeightRatio, 0f);
            return;
        }

        if (!isGameOver)
        {
            return;
        }

        if (gameOverImage == null && gameOverSpriteRenderer == null)
        {
            float yOffset = GetDropBounceOffset(Time.unscaledTime - gameOverStartedAt);
            DrawSprite(gameOverSprite, gameOverScreenWidthRatio, gameOverScreenHeightRatio, yOffset);
        }

        if (!retryDeclined && Time.unscaledTime - gameOverStartedAt >= dropDuration + bounceDuration)
        {
            DrawRetryButtons();
        }
    }

    float GetDropBounceOffset(float elapsed)
    {
        if (elapsed < dropDuration)
        {
            float t = Mathf.Clamp01(elapsed / dropDuration);
            return Mathf.Lerp(-Screen.height, 0f, t * t * t);
        }

        float bounceTime = elapsed - dropDuration;
        if (bounceTime >= bounceDuration)
        {
            return 0f;
        }

        float b = Mathf.Clamp01(bounceTime / bounceDuration);
        return -Mathf.Abs(Mathf.Sin(b * Mathf.PI * bounceCount)) * bounceHeight * (1f - b);
    }

    void DrawRetryButtons()
    {
        float width = Screen.width * 0.45f;
        float height = Screen.height * 0.18f;
        float x = (Screen.width - width) * 0.5f;
        float y = Screen.height * 0.62f;

        GUI.Box(new Rect(x, y, width, height), "リトライしますか？");

        if (GUI.Button(new Rect(x + width * 0.16f, y + height * 0.55f, width * 0.3f, height * 0.3f), "はい"))
        {
            StartRetry();
        }

        if (GUI.Button(new Rect(x + width * 0.54f, y + height * 0.55f, width * 0.3f, height * 0.3f), "いいえ"))
        {
            retryDeclined = true;
        }
    }

    void DrawSprite(Sprite sprite, float widthRatio, float heightRatio, float yOffset)
    {
        if (sprite == null)
        {
            return;
        }

        Texture2D texture = sprite.texture;
        Rect spriteRect = sprite.textureRect;
        float scale = Mathf.Min(Screen.width * widthRatio / spriteRect.width, Screen.height * heightRatio / spriteRect.height);
        Rect drawRect = new Rect((Screen.width - spriteRect.width * scale) * 0.5f, (Screen.height - spriteRect.height * scale) * 0.5f + yOffset, spriteRect.width * scale, spriteRect.height * scale);
        Rect uv = new Rect(spriteRect.x / texture.width, spriteRect.y / texture.height, spriteRect.width / texture.width, spriteRect.height / texture.height);
        GUI.DrawTextureWithTexCoords(drawRect, texture, uv, true);
    }

    void StartRetry()
    {
        if (!isRetrying)
        {
            StartCoroutine(RetryAfterReadyBang());
        }
    }

    IEnumerator RetryAfterReadyBang()
    {
        isRetrying = true;
        isShowingReadyBang = true;
        isGameOver = false;

        yield return new WaitForSecondsRealtime(readyBangDuration);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void SetVisible(Graphic target, bool visible)
    {
        if (target != null)
        {
            target.gameObject.SetActive(visible);
        }
    }

    void SetVisible(SpriteRenderer target, bool visible)
    {
        if (target != null)
        {
            target.gameObject.SetActive(visible);
        }
    }

    void OnValidate()
    {
        dropDuration = Mathf.Max(0.01f, dropDuration);
        bounceDuration = Mathf.Max(0.01f, bounceDuration);
        bounceHeight = Mathf.Max(0f, bounceHeight);
        bounceCount = Mathf.Max(1, bounceCount);
        gameOverScreenWidthRatio = Mathf.Clamp01(gameOverScreenWidthRatio);
        gameOverScreenHeightRatio = Mathf.Clamp01(gameOverScreenHeightRatio);
        playerKnockbackDuration = Mathf.Max(0.01f, playerKnockbackDuration);
        playerKnockbackHeight = Mathf.Max(0f, playerKnockbackHeight);
        playerExitViewportPadding = Mathf.Max(0f, playerExitViewportPadding);
        playerExitScaleMultiplier = Mathf.Max(0.01f, playerExitScaleMultiplier);
        readyBangDuration = Mathf.Max(0.01f, readyBangDuration);
        readyBangScreenWidthRatio = Mathf.Clamp01(readyBangScreenWidthRatio);
        readyBangScreenHeightRatio = Mathf.Clamp01(readyBangScreenHeightRatio);
    }
}
