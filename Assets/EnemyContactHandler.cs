using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyContactHandler : MonoBehaviour
{
    [SerializeField] GameOverController gameOverController;
    [SerializeField] SpriteRenderer enemySpriteRenderer;
    [SerializeField] Sprite stompedSprite;
    [SerializeField] float stompCheckHeight = 0.15f;
    [SerializeField] float stompBouncePower = 6f;
    [SerializeField] float hideAfterStompSeconds = 0.5f;
    [SerializeField] float stompedSpriteDrop = 0.12f;

    [Header("Stomp Impact")]
    [SerializeField] Sprite stompImpactSprite;
    [SerializeField] Vector2 stompImpactOffset = new Vector2(0f, 0.15f);
    [SerializeField] float stompImpactStartScale = 0.55f;
    [SerializeField] float stompImpactEndScale = 0.9f;
    [SerializeField] float stompImpactDuration = 0.35f;
    [SerializeField] int stompImpactSortingOrder = 19;

    bool isStomped;

    void Start()
    {
        if (enemySpriteRenderer == null)
        {
            enemySpriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (gameOverController == null)
        {
            gameOverController = FindFirstObjectByType<GameOverController>();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HitPlayer(collision.gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        HitPlayer(collision.gameObject);
    }

    void HitPlayer(GameObject player)
    {
        if (isStomped || player == null || !player.CompareTag("Player"))
        {
            return;
        }

        if (IsStompedBy(player))
        {
            Stomp(player);
            return;
        }

        if (gameOverController == null)
        {
            return;
        }

        Vector2 direction = player.transform.position - transform.position;
        if (Mathf.Abs(direction.x) < 0.01f)
        {
            direction.x = 1f;
        }

        gameOverController.TriggerGameOver(player, new Vector2(direction.x, 0f).normalized);
    }

    bool IsStompedBy(GameObject player)
    {
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        Collider2D enemyCollider = GetComponent<Collider2D>();
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();

        if (playerCollider == null || enemyCollider == null || playerRb == null)
        {
            return false;
        }

        bool playerIsFalling = playerRb.linearVelocity.y <= 0.1f;
        bool playerIsAbove = playerCollider.bounds.min.y >= enemyCollider.bounds.max.y - stompCheckHeight;
        return playerIsFalling && playerIsAbove;
    }

    void Stomp(GameObject player)
    {
        isStomped = true;

        SlimePatrolMotion motion = GetComponent<SlimePatrolMotion>();
        if (motion != null)
        {
            motion.enabled = false;
        }

        Rigidbody2D enemyRb = GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            enemyRb.linearVelocity = Vector2.zero;
            enemyRb.simulated = false;
        }

        foreach (Collider2D enemyCollider in GetComponents<Collider2D>())
        {
            enemyCollider.enabled = false;
        }

        if (enemySpriteRenderer != null && stompedSprite != null)
        {
            enemySpriteRenderer.sprite = stompedSprite;
            enemySpriteRenderer.transform.localPosition += Vector3.down * stompedSpriteDrop;
        }

        ShowStompImpact(player);

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, stompBouncePower);
        }

        Invoke(nameof(HideEnemy), hideAfterStompSeconds);
    }

    void HideEnemy()
    {
        gameObject.SetActive(false);
    }

    void ShowStompImpact(GameObject player)
    {
        if (stompImpactSprite == null || player == null)
        {
            return;
        }

        GameObject impact = new GameObject("StompImpact");
        impact.transform.position = GetStompImpactPosition(player);
        impact.transform.localScale = Vector3.one * stompImpactStartScale;

        SpriteRenderer impactRenderer = impact.AddComponent<SpriteRenderer>();
        impactRenderer.sprite = stompImpactSprite;
        impactRenderer.sortingOrder = stompImpactSortingOrder;

        StartCoroutine(AnimateStompImpact(impactRenderer));
    }

    Vector3 GetStompImpactPosition(GameObject player)
    {
        Collider2D enemyCollider = GetComponent<Collider2D>();
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        float x = transform.position.x + stompImpactOffset.x;
        float y = player.transform.position.y + stompImpactOffset.y;

        if (enemyCollider != null && playerCollider != null)
        {
            x = (enemyCollider.bounds.center.x + playerCollider.bounds.center.x) * 0.5f + stompImpactOffset.x;
            y = (enemyCollider.bounds.max.y + playerCollider.bounds.min.y) * 0.5f + stompImpactOffset.y;
        }
        else if (enemyCollider != null)
        {
            x = enemyCollider.bounds.center.x + stompImpactOffset.x;
            y = enemyCollider.bounds.max.y + stompImpactOffset.y;
        }

        return new Vector3(x, y, transform.position.z);
    }

    IEnumerator AnimateStompImpact(SpriteRenderer impactRenderer)
    {
        float timer = 0f;
        Color baseColor = impactRenderer.color;
        Transform impactTransform = impactRenderer.transform;

        while (timer < stompImpactDuration)
        {
            float t = Mathf.Clamp01(timer / stompImpactDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            float scale = Mathf.Lerp(stompImpactStartScale, stompImpactEndScale, eased);
            impactTransform.localScale = Vector3.one * scale;
            impactRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, Mathf.Lerp(1f, 0f, t));
            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(impactTransform.gameObject);
    }

    void OnValidate()
    {
        stompCheckHeight = Mathf.Max(0f, stompCheckHeight);
        stompBouncePower = Mathf.Max(0f, stompBouncePower);
        hideAfterStompSeconds = Mathf.Max(0f, hideAfterStompSeconds);
        stompedSpriteDrop = Mathf.Max(0f, stompedSpriteDrop);
        stompImpactStartScale = Mathf.Max(0.01f, stompImpactStartScale);
        stompImpactEndScale = Mathf.Max(0.01f, stompImpactEndScale);
        stompImpactDuration = Mathf.Max(0.01f, stompImpactDuration);
    }
}
