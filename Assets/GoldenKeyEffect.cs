using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GoldenKeyEffect : MonoBehaviour
{
    [SerializeField] float hoverHeight = 0.25f;
    [SerializeField] float hoverSpeed = 1.5f;
    [SerializeField] float tiltAmount = 4f;

    Vector3 startPosition;
    Quaternion startRotation;

    void Start()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }

    void Update()
    {
        float wave = Mathf.Sin(Time.time * hoverSpeed);
        transform.localPosition = startPosition + Vector3.up * wave * hoverHeight;
        transform.localRotation = startRotation * Quaternion.Euler(0f, 0f, wave * tiltAmount);
    }

    void OnValidate()
    {
        hoverHeight = Mathf.Max(0f, hoverHeight);
        hoverSpeed = Mathf.Max(0f, hoverSpeed);
        tiltAmount = Mathf.Max(0f, tiltAmount);
    }
}
