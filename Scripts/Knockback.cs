// ==== Knockback.cs ====
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Knockback : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float knockbackResistance = 0f; // 0 = нет сопротивления, 1 = полный иммунитет

    private Rigidbody2D rb;
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;

    public bool IsKnockedBack => isKnockedBack;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;
                rb.velocity = Vector2.zero;
            }
        }
    }

    /// <summary>
    /// Применить отталкивание
    /// </summary>
    /// <param name="sourcePosition">Откуда толкает (центр взрыва)</param>
    /// <param name="force">Сила толчка</param>
    /// <param name="duration">Длительность состояния отталкивания</param>
    public void Apply(Vector2 sourcePosition, float force, float duration = 0.2f)
    {
        if (knockbackResistance >= 1f) return;

        float finalForce = force * (1f - knockbackResistance);

        Vector2 direction = ((Vector2)transform.position - sourcePosition).normalized;

        // Если объект точно в центре взрыва — случайное направление
        if (direction == Vector2.zero)
            direction = Random.insideUnitCircle.normalized;

        rb.velocity = Vector2.zero;
        rb.AddForce(direction * finalForce, ForceMode2D.Impulse);

        isKnockedBack = true;
        knockbackTimer = duration;
    }

    /// <summary>
    /// Применить отталкивание с учётом расстояния
    /// </summary>
    public void ApplyWithFalloff(Vector2 sourcePosition, float maxForce, float radius, float duration = 0.2f)
    {
        if (knockbackResistance >= 1f) return;

        float distance = Vector2.Distance(transform.position, sourcePosition);

        if (distance > radius) return;

        // Чем ближе к центру — тем сильнее
        float falloff = 1f - (distance / radius);
        float force = maxForce * falloff;

        Apply(sourcePosition, force, duration);
    }
}