// ==== BaseEnemy.cs ====
using UnityEngine;
using System.Collections;

public abstract class BaseEnemy : MonoBehaviour, IDamageable
{
    [Header("Stats (заполняются из SO)")]
    public float maxHealth = 5f;
    public float moveSpeed = 2f;
    public float damage = 1f;
    public float contactDamage = 1f;

    [Header("Contact Damage")]
    [SerializeField] protected float contactRadius = 0.32f;
    [SerializeField] protected float contactCooldown = 0.5f;
    protected float contactTimer = 0f;

    // Компоненты
    protected float currentHealth;
    protected Room room;
    protected Transform player;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected bool isDead = false;

    // Данные из SO (сохраняем для применения в Start)
    private EnemyDataSO pendingData;
    private int pendingLevel;
    private bool hasPendingData = false;

    public bool IsAlive => currentHealth > 0 && !isDead;

    // ==================== ИНИЦИАЛИЗАЦИЯ ====================

    public virtual void Initialize(EnemyDataSO data, int level)
    {
        // Сохраняем данные — применим когда компоненты будут готовы
        pendingData = data;
        pendingLevel = level;
        hasPendingData = true;

        // Пробуем применить сразу если компоненты уже есть
        TryApplyData();
    }

    private void TryApplyData()
    {
        if (!hasPendingData || pendingData == null) return;

        // Проверяем что компоненты готовы
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null) return; // Ещё не готов, применим в Start

        // Применяем статы
        maxHealth = pendingData.GetHealth(pendingLevel);
        damage = pendingData.GetDamage(pendingLevel);
        contactDamage = damage;
        moveSpeed = pendingData.moveSpeed;
        currentHealth = maxHealth;

        // Применяем спрайт
        if (pendingData.sprite != null)
        {
            spriteRenderer.sprite = pendingData.sprite;
            spriteRenderer.color = Color.white;
            Debug.Log($"[{pendingData.enemyName}] Sprite applied: {pendingData.sprite.name}");
        }

        Debug.Log($"[{pendingData.enemyName}] HP: {maxHealth}, DMG: {damage}, SPD: {moveSpeed} (Lvl {pendingLevel})");

        hasPendingData = false;
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Если данные уже были переданы до Awake
        TryApplyData();
    }

    protected virtual void Start()
    {
        // Последняя попытка применить данные
        TryApplyData();

        if (currentHealth <= 0)
            currentHealth = maxHealth;

        room = GetComponentInParent<Room>();

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            player = playerGO.transform;
    }

    // ==================== УРОН ====================

    public virtual void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        currentHealth -= amount;

        StartCoroutine(FlashRed());

        Debug.Log($"{name} took {amount} dmg. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        OnDeath();

        if (room != null)
            room.EnemyDefeated(gameObject);

        Destroy(gameObject);
    }

    protected virtual void OnDeath() { }

    // ==================== КОНТАКТНЫЙ УРОН ====================

    protected virtual void HandleContactDamage()
    {
        if (contactTimer > 0)
        {
            contactTimer -= Time.fixedDeltaTime;
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, contactRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth ph = hit.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(contactDamage);
                    contactTimer = contactCooldown;
                }
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAlive) return;
        if (!other.CompareTag("Player")) return;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph != null)
            ph.TakeDamage(contactDamage);
    }

    // ==================== ХЕЛПЕРЫ ====================

    protected float DistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        return Vector2.Distance(transform.position, player.position);
    }

    protected Vector2 DirectionToPlayer()
    {
        if (player == null) return Vector2.zero;
        return ((Vector2)(player.position - transform.position)).normalized;
    }

    protected Vector2 DirectionFromPlayer()
    {
        return -DirectionToPlayer();
    }

    protected void MoveTowards(Vector2 direction, float speed)
    {
        rb.MovePosition(rb.position + direction * (speed * Time.fixedDeltaTime));
    }

    protected void FacePlayer()
    {
        if (player == null || spriteRenderer == null) return;
        float dir = player.position.x - transform.position.x;
        if (Mathf.Abs(dir) > 0.1f)
        {
            spriteRenderer.flipX = dir < 0;
        }
    }

    protected IEnumerator FlashRed()
    {
        if (spriteRenderer == null) yield break;

        Color original = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if (spriteRenderer != null)
            spriteRenderer.color = original;
    }
}