using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollowController : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] Transform player;
    [SerializeField] SpriteRenderer backgroundRenderer;
    [SerializeField] string backgroundObjectName = "JungleMainBackground";
    [SerializeField] float startFollowOffsetFromCenter = 0.5f;
    [SerializeField] float smoothTime = 0.15f;
    [SerializeField] float horizontalPadding = 0f;
    [SerializeField] bool followY = false;
    [SerializeField] float verticalOffset = 0f;

    [Header("Far Background")]
    [SerializeField] SpriteRenderer parallaxBackgroundRenderer;
    [SerializeField] string parallaxBackgroundObjectName = "JungleFarBackground";
    [SerializeField] float parallaxMovementRatio = 0.35f;
    [SerializeField] float parallaxYOffset = 0f;

    [Header("Foreground Parallax")]
    [SerializeField] SpriteRenderer foregroundParallaxRenderer;
    [SerializeField] string foregroundParallaxObjectName = "ForegroundParallaxTree";
    [SerializeField] float foregroundParallaxMovementRatio = -0.18f;
    [SerializeField] float foregroundParallaxYOffset = 0f;

    [Header("Start Ready")]
    [SerializeField] bool showReadyBangBeforeStart = true;
    [SerializeField] Sprite startReadyBangSprite;
    [SerializeField] string startButtonText = "スタート";
    [SerializeField] float startButtonWidthRatio = 0.28f;
    [SerializeField] float startButtonHeightRatio = 0.09f;
    [SerializeField] float startButtonYRatio = 0.78f;
    [SerializeField] float startButtonFontSizeRatio = 0.028f;
    [SerializeField] float startDarkOverlayAlpha = 0.65f;
    [SerializeField] float startBrightenDuration = 0.45f;
    [SerializeField] float startReadyImageWidthRatio = 0.75f;
    [SerializeField] float startReadyImageHeightRatio = 0.28f;

    Camera targetCamera;
    Vector3 startPosition;
    Vector3 foregroundParallaxStartPosition;
    float xVelocity;
    float yVelocity;
    bool isFollowing;
    bool hasForegroundParallaxStartPosition;
    bool waitingForStart;
    bool brightening;
    float brightenStartedAt;
    PlayerMovement playerMovement;
    PlayerJumpController playerJump;
    bool playerMovementWasEnabled;
    bool playerJumpWasEnabled;

    void Awake()
    {
        targetCamera = GetComponent<Camera>();
        startPosition = transform.position;
    }

    void Start()
    {
        FindMissingReferences();
        MoveCameraInsideBackground();
        startPosition = transform.position;
        StoreForegroundParallaxStartPosition();
        UpdateFarBackground();
        UpdateForegroundParallax();
        PrepareReadyStart();
    }

    void LateUpdate()
    {
        if (brightening && Time.unscaledTime - brightenStartedAt >= startBrightenDuration)
        {
            StartGame();
        }

        if (player == null)
        {
            return;
        }

        if (!isFollowing && player.position.x >= startPosition.x - startFollowOffsetFromCenter)
        {
            isFollowing = true;
        }

        float targetX = isFollowing ? player.position.x : startPosition.x;
        float targetY = followY ? player.position.y + verticalOffset : startPosition.y;
        Vector3 position = transform.position;

        targetX = ClampCameraX(targetX);

        if (smoothTime <= 0f)
        {
            position.x = targetX;
            position.y = targetY;
        }
        else
        {
            position.x = Mathf.SmoothDamp(position.x, targetX, ref xVelocity, smoothTime);
            position.y = Mathf.SmoothDamp(position.y, targetY, ref yVelocity, smoothTime);
        }

        transform.position = new Vector3(position.x, position.y, startPosition.z);
        UpdateFarBackground();
        UpdateForegroundParallax();
    }

    void FindMissingReferences()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            player = playerObject != null ? playerObject.transform : null;
        }

        if (backgroundRenderer == null)
        {
            GameObject backgroundObject = GameObject.Find(backgroundObjectName);
            backgroundRenderer = backgroundObject != null ? backgroundObject.GetComponent<SpriteRenderer>() : null;
        }

        if (parallaxBackgroundRenderer == null)
        {
            GameObject parallaxObject = GameObject.Find(parallaxBackgroundObjectName);
            parallaxBackgroundRenderer = parallaxObject != null ? parallaxObject.GetComponent<SpriteRenderer>() : null;
        }

        if (foregroundParallaxRenderer == null)
        {
            GameObject foregroundObject = GameObject.Find(foregroundParallaxObjectName);
            foregroundParallaxRenderer = foregroundObject != null ? foregroundObject.GetComponent<SpriteRenderer>() : null;
        }
    }

    void MoveCameraInsideBackground()
    {
        Vector3 position = transform.position;
        position.x = ClampCameraX(position.x);
        transform.position = position;
    }

    float ClampCameraX(float x)
    {
        if (targetCamera == null || backgroundRenderer == null || !targetCamera.orthographic)
        {
            return x;
        }

        float halfWidth = targetCamera.orthographicSize * targetCamera.aspect;
        Bounds bounds = backgroundRenderer.bounds;
        float minX = bounds.min.x + halfWidth + horizontalPadding;
        float maxX = bounds.max.x - halfWidth - horizontalPadding;

        if (minX > maxX)
        {
            return bounds.center.x;
        }

        return Mathf.Clamp(x, minX, maxX);
    }

    void UpdateFarBackground()
    {
        if (targetCamera == null || backgroundRenderer == null || parallaxBackgroundRenderer == null || !targetCamera.orthographic)
        {
            return;
        }

        Bounds mainBounds = backgroundRenderer.bounds;
        float halfWidth = targetCamera.orthographicSize * targetCamera.aspect;
        float minX = mainBounds.min.x + halfWidth;
        float maxX = mainBounds.max.x - halfWidth;
        float progress = Mathf.Approximately(minX, maxX) ? 0.5f : Mathf.InverseLerp(minX, maxX, transform.position.x);

        Bounds farBounds = parallaxBackgroundRenderer.bounds;
        float extraWidth = Mathf.Max(0f, farBounds.size.x - halfWidth * 2f);
        float offsetX = Mathf.Lerp(extraWidth * 0.5f, -extraWidth * 0.5f, progress) * parallaxMovementRatio;
        Vector3 farPosition = parallaxBackgroundRenderer.transform.position;
        farPosition.x = transform.position.x + offsetX;
        farPosition.y = transform.position.y + parallaxYOffset;
        parallaxBackgroundRenderer.transform.position = farPosition;
    }

    void StoreForegroundParallaxStartPosition()
    {
        if (foregroundParallaxRenderer == null)
        {
            return;
        }

        foregroundParallaxStartPosition = foregroundParallaxRenderer.transform.position;
        hasForegroundParallaxStartPosition = true;
    }

    void UpdateForegroundParallax()
    {
        if (foregroundParallaxRenderer == null)
        {
            return;
        }

        if (!hasForegroundParallaxStartPosition)
        {
            StoreForegroundParallaxStartPosition();
        }

        Vector3 cameraDelta = transform.position - startPosition;
        Vector3 foregroundPosition = foregroundParallaxStartPosition;
        foregroundPosition.x += cameraDelta.x * foregroundParallaxMovementRatio;
        foregroundPosition.y += foregroundParallaxYOffset;
        foregroundParallaxRenderer.transform.position = foregroundPosition;
    }

    void PrepareReadyStart()
    {
        if (!showReadyBangBeforeStart || player == null)
        {
            return;
        }

        playerMovement = player.GetComponent<PlayerMovement>();
        playerJump = player.GetComponent<PlayerJumpController>();
        playerMovementWasEnabled = playerMovement != null && playerMovement.enabled;
        playerJumpWasEnabled = playerJump != null && playerJump.enabled;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerJump != null)
        {
            playerJump.enabled = false;
        }

        Time.timeScale = 0f;
        waitingForStart = true;
    }

    void OnGUI()
    {
        if (!waitingForStart && !brightening)
        {
            return;
        }

        DrawDimOverlay(GetOverlayAlpha());

        if (waitingForStart)
        {
            DrawReadyImage();
            DrawStartButton();
        }
    }

    void DrawDimOverlay(float alpha)
    {
        Color previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, alpha);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    float GetOverlayAlpha()
    {
        if (!brightening)
        {
            return startDarkOverlayAlpha;
        }

        float t = Mathf.Clamp01((Time.unscaledTime - brightenStartedAt) / startBrightenDuration);
        return Mathf.Lerp(startDarkOverlayAlpha, 0f, t);
    }

    void DrawReadyImage()
    {
        if (startReadyBangSprite == null)
        {
            return;
        }

        DrawSprite(startReadyBangSprite, Mathf.Min(startReadyImageWidthRatio, 0.75f), Mathf.Min(startReadyImageHeightRatio, 0.28f), 0.36f);
    }

    void DrawStartButton()
    {
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = Mathf.RoundToInt(Screen.height * startButtonFontSizeRatio);

        float width = Screen.width * startButtonWidthRatio;
        float height = Screen.height * startButtonHeightRatio;
        float x = (Screen.width - width) * 0.5f;
        float y = Screen.height * startButtonYRatio;

        if (GUI.Button(new Rect(x, y, width, height), startButtonText, style))
        {
            waitingForStart = false;
            brightening = true;
            brightenStartedAt = Time.unscaledTime;
        }
    }

    void DrawSprite(Sprite sprite, float widthRatio, float heightRatio, float centerYRatio)
    {
        Texture2D texture = sprite.texture;
        Rect spriteRect = sprite.textureRect;
        float scale = Mathf.Min(Screen.width * widthRatio / spriteRect.width, Screen.height * heightRatio / spriteRect.height);
        Rect drawRect = new Rect((Screen.width - spriteRect.width * scale) * 0.5f, Screen.height * centerYRatio - spriteRect.height * scale * 0.5f, spriteRect.width * scale, spriteRect.height * scale);
        Rect uv = new Rect(spriteRect.x / texture.width, spriteRect.y / texture.height, spriteRect.width / texture.width, spriteRect.height / texture.height);
        GUI.DrawTextureWithTexCoords(drawRect, texture, uv, true);
    }

    void StartGame()
    {
        brightening = false;
        Time.timeScale = 1f;

        if (playerMovement != null)
        {
            playerMovement.enabled = playerMovementWasEnabled;
        }

        if (playerJump != null)
        {
            playerJump.enabled = playerJumpWasEnabled;
        }
    }

    void OnValidate()
    {
        startFollowOffsetFromCenter = Mathf.Max(0f, startFollowOffsetFromCenter);
        smoothTime = Mathf.Max(0f, smoothTime);
        horizontalPadding = Mathf.Max(0f, horizontalPadding);
        parallaxMovementRatio = Mathf.Clamp01(parallaxMovementRatio);
        foregroundParallaxMovementRatio = Mathf.Clamp(foregroundParallaxMovementRatio, -1f, 1f);
        startButtonWidthRatio = Mathf.Clamp01(startButtonWidthRatio);
        startButtonHeightRatio = Mathf.Clamp01(startButtonHeightRatio);
        startButtonYRatio = Mathf.Clamp01(startButtonYRatio);
        startButtonFontSizeRatio = Mathf.Clamp(startButtonFontSizeRatio, 0.01f, 0.08f);
        startDarkOverlayAlpha = Mathf.Clamp01(startDarkOverlayAlpha);
        startBrightenDuration = Mathf.Max(0.01f, startBrightenDuration);
        startReadyImageWidthRatio = Mathf.Clamp01(startReadyImageWidthRatio);
        startReadyImageHeightRatio = Mathf.Clamp01(startReadyImageHeightRatio);
    }
}
