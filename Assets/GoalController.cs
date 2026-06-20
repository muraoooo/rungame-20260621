using UnityEngine;
using UnityEngine.UI;

public class GoalController : MonoBehaviour
{
    [Header("Goal Image")]
    [SerializeField] Image goalImage;
    [SerializeField] SpriteRenderer goalSpriteRenderer;
    [SerializeField] Sprite goalSprite;
    [SerializeField] bool stopGameTimeOnGoal = true;

    [Header("Player Bounce")]
    [SerializeField] Sprite goalPlayerJumpSprite;
    [SerializeField] float playerBounceHeight = 0.25f;
    [SerializeField] float playerBounceSpeed = 2.4f;

    [Header("Screen")]
    [SerializeField] Color overlayColor = new Color(0f, 0f, 0f, 0.62f);
    [SerializeField] float goalScreenWidthRatio = 0.85f;
    [SerializeField] float goalScreenHeightRatio = 0.45f;

    bool reachedGoal;
    float goalTime;
    Transform goalPlayer;
    Vector3 playerBasePosition;

    void Start()
    {
        SetVisible(goalImage, false);
        SetVisible(goalSpriteRenderer, false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        ReachGoal(collision.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        ReachGoal(collision.gameObject);
    }

    void ReachGoal(GameObject player)
    {
        if (reachedGoal || player == null || !player.CompareTag("Player"))
        {
            return;
        }

        reachedGoal = true;
        goalTime = Time.unscaledTime;
        goalPlayer = player.transform;
        playerBasePosition = player.transform.position;

        ShowGoalObjects();
        StopPlayer(player);
        ShowGoalPlayerSprite(player);

        if (stopGameTimeOnGoal)
        {
            Time.timeScale = 0f;
        }
    }

    void ShowGoalObjects()
    {
        SetVisible(goalImage, true);
        SetVisible(goalSpriteRenderer, true);

        if (goalImage != null && goalSprite != null)
        {
            goalImage.sprite = goalSprite;
        }

        if (goalSpriteRenderer != null && goalSprite != null)
        {
            goalSpriteRenderer.sprite = goalSprite;
        }
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
            rb2d.simulated = false;
        }
    }

    void ShowGoalPlayerSprite(GameObject player)
    {
        if (goalPlayerJumpSprite == null)
        {
            return;
        }

        Animator animator = player.GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }

        SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = goalPlayerJumpSprite;
        }
    }

    void Update()
    {
        if (!reachedGoal || goalPlayer == null)
        {
            return;
        }

        float wave = Mathf.Abs(Mathf.Sin((Time.unscaledTime - goalTime) * playerBounceSpeed * Mathf.PI));
        goalPlayer.position = playerBasePosition + Vector3.up * wave * playerBounceHeight;
    }

    void OnGUI()
    {
        if (!reachedGoal)
        {
            return;
        }

        Color previousColor = GUI.color;
        GUI.color = overlayColor;
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;

        if (goalImage == null && goalSpriteRenderer == null)
        {
            DrawSprite(goalSprite, goalScreenWidthRatio, goalScreenHeightRatio);
        }
    }

    void DrawSprite(Sprite sprite, float widthRatio, float heightRatio)
    {
        if (sprite == null)
        {
            return;
        }

        Texture2D texture = sprite.texture;
        Rect spriteRect = sprite.textureRect;
        float scale = Mathf.Min(Screen.width * widthRatio / spriteRect.width, Screen.height * heightRatio / spriteRect.height);
        Rect drawRect = new Rect((Screen.width - spriteRect.width * scale) * 0.5f, (Screen.height - spriteRect.height * scale) * 0.5f, spriteRect.width * scale, spriteRect.height * scale);
        Rect uv = new Rect(spriteRect.x / texture.width, spriteRect.y / texture.height, spriteRect.width / texture.width, spriteRect.height / texture.height);
        GUI.DrawTextureWithTexCoords(drawRect, texture, uv, true);
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
        playerBounceHeight = Mathf.Max(0f, playerBounceHeight);
        playerBounceSpeed = Mathf.Max(0f, playerBounceSpeed);
        goalScreenWidthRatio = Mathf.Clamp01(goalScreenWidthRatio);
        goalScreenHeightRatio = Mathf.Clamp01(goalScreenHeightRatio);
    }
}
