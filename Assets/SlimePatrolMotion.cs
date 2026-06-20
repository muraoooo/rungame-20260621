using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SlimePatrolMotion : MonoBehaviour
{
    private enum StartDirection
    {
        Left = -1,
        Right = 1
    }

    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private StartDirection startDirection = StartDirection.Left;
    [SerializeField] private float patrolDistance = 12f;

    private Rigidbody2D rb2d;
    private float startX;
    private float direction;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        startX = rb2d.position.x;
        direction = (float)startDirection;
    }

    private void FixedUpdate()
    {
        float distanceFromStart = rb2d.position.x - startX;

        if (Mathf.Abs(distanceFromStart) >= patrolDistance)
        {
            direction = distanceFromStart > 0f ? -1f : 1f;
        }

        rb2d.linearVelocity = new Vector2(direction * moveSpeed, rb2d.linearVelocity.y);
    }

    private void OnDisable()
    {
        if (rb2d != null)
        {
            rb2d.linearVelocity = new Vector2(0f, rb2d.linearVelocity.y);
        }
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0f, moveSpeed);
        patrolDistance = Mathf.Max(0f, patrolDistance);
    }
}
