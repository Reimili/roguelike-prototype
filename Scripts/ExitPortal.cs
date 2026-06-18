using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class ExitPortal : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseMin = 0.6f;
    [SerializeField] private float pulseMax = 1f;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.black;

        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    void Update()
    {
        // Пульсация прозрачности
        float alpha = Mathf.Lerp(pulseMin, pulseMax,
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0, 0, 0, alpha);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("[ExitPortal] Player entered! Loading next level...");

        LevelManager.Instance?.LoadNextLevel();
    }
}