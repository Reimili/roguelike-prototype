// ==== DummyEnemy.cs ====
using UnityEngine;

public class DummyEnemy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float maxHealth = 5f;
    public float moveSpeed = 2f;
    public int damage = 1;

    [Header("Movement")]
    public float stopDistance = 0.6f;
    public float deviationInterval = 0.5f;
    public float deviationAngle = 30f;

    private float currentHealth;
    private Room room;
    private Transform player;
    private Rigidbody2D rb;

    private float deviationTimer = 0f;
    private Vector2 currentDir = Vector2.zero;
    private bool isDead = false;  // Защита от повторной смерти

    public bool IsAlive => currentHealth > 0 && !isDead;

    void Start()
    {
        currentHealth = maxHealth;
        room = GetComponentInParent<Room>();
        rb = GetComponent<Rigidbody2D>();

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            player = playerGO.transform;
    }

    void FixedUpdate()
    {
        if (player == null || !IsAlive) return;

        Vector2 toPlayer = (player.position - transform.position);
        float dist = toPlayer.magnitude;

        // Отклонение движения
        deviationTimer -= Time.fixedDeltaTime;
        if (deviationTimer <= 0f)
        {
            float angle = Random.Range(-deviationAngle, deviationAngle);
            currentDir = Quaternion.Euler(0, 0, angle) * toPlayer.normalized;
            deviationTimer = deviationInterval;
        }

        // Контактный урон
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.32f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth ph = hit.GetComponent<PlayerHealth>();
                if (ph != null)
                    ph.TakeDamage(damage);
            }
        }

        // Движение
        if (dist > stopDistance)
        {
            rb.MovePosition(rb.position + currentDir * (moveSpeed * Time.fixedDeltaTime));
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        currentHealth -= amount;
        Debug.Log($"{name} took {amount} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (isDead) return;  // Защита от повторного вызова
        isDead = true;

        Debug.Log($"[DummyEnemy] {name} died!");

        // Уведомляем комнату
        if (room != null)
            room.EnemyDefeated(gameObject);

        // ВСЕГДА уничтожаем объект
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAlive) return;
    }
}