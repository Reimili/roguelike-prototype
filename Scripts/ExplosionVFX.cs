using UnityEngine;

public class ExplosionVFX : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private float startScale = 0.2f;

    [Header("Colors")]
    [SerializeField] private Color startColor = new Color(1f, 0.85f, 0.3f, 0.95f);
    [SerializeField] private Color endColor = new Color(1f, 0.2f, 0f, 0f);

    private float timer = 0f;
    private float targetScale = 1f;
    private bool isPlaying = false;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Play(float radius)
    {
        targetScale = radius * 2f;
        timer = 0f;
        isPlaying = true;

        transform.localScale = Vector3.one * startScale;

        if (spriteRenderer != null)
            spriteRenderer.color = startColor;
    }

    private void Update()
    {
        if (!isPlaying) return;

        timer += Time.deltaTime;
        float t = timer / duration;

        transform.localScale = Vector3.one * Mathf.Lerp(startScale, targetScale, t);

        if (spriteRenderer != null)
            spriteRenderer.color = Color.Lerp(startColor, endColor, t);

        if (t >= 1f)
            Destroy(gameObject);
    }
}