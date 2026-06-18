// ==== PlayerHealth.cs ====
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Invincibility")]
    [SerializeField] private float invincibilityTime = 1f;
    [SerializeField] private Color invincibleColor = new Color(1f, 1f, 1f, 0.4f);

    private SpriteRenderer spriteRenderer;
    private float currentHealth;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    private PlayerStats stats;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => stats != null ? stats.GetStat(StatType.MaxHealth) : 5f;

    private float previousMaxHealth;
    public bool IsAlive => currentHealth > 0;

    public event System.Action<float, float> OnHealthChanged;

    void Awake()
    {
        // —инглтон
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[PlayerHealth] Instance created");
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] Duplicate instance destroyed");
            Destroy(gameObject);
            return;
        }

        stats = GetComponent<PlayerStats>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (stats == null)
        {
            Debug.LogError("[PlayerHealth] PlayerStats component not found!");
        }
    }

    void Start()
    {
        currentHealth = MaxHealth;
        previousMaxHealth = MaxHealth;

        OnHealthChanged?.Invoke(currentHealth, MaxHealth);

        if (stats != null)
        {
            stats.OnStatsChanged += OnStatsUpdated;
        }
    }

    void OnDestroy()
    {
        if (stats != null)
            stats.OnStatsChanged -= OnStatsUpdated;
    }

    void Update()
    {
        // “аймер неу€звимости
        if (isInvincible && invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                SetColor(Color.white);
            }
        }

        // –еген здоровь€
        if (stats != null)
        {
            float regen = stats.GetStat(StatType.HealthRegen);
            if (regen > 0 && currentHealth < MaxHealth)
            {
                Heal(regen * Time.deltaTime);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isInvincible || !IsAlive) return;

        // ≈сли урон 0 или меньше Ч игнорируем полностью
        if (amount <= 0) return;

        // ”клонение
        if (stats != null)
        {
            float dodgeChance = stats.GetStat(StatType.DodgeChance);
            if (dodgeChance > 0 && Random.value < dodgeChance)
            {
                Debug.Log("[PlayerHealth] Dodged!");
                return;
            }
        }

        // Ѕрон€
        float armor = stats != null ? stats.GetStat(StatType.Armor) : 0f;
        float finalDamage = Mathf.Max(0, amount - armor);

        // Ёффекты предметов
        var inventory = GetComponent<Inventory>();
        if (inventory != null)
            inventory.NotifyTakeDamage(ref finalDamage);

        currentHealth -= finalDamage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"[PlayerHealth] Took {finalDamage} damage. HP: {currentHealth}/{MaxHealth}");
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartInvincibility(invincibilityTime);
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;

        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, MaxHealth);

        if (currentHealth != oldHealth)
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    public void StartInvincibility(float duration)
    {
        isInvincible = true;
        invincibilityTimer = duration;
        SetColor(invincibleColor);
    }

    public void SetInvincible(bool value)
    {
        isInvincible = value;
        SetColor(value ? invincibleColor : Color.white);
        if (value) invincibilityTimer = invincibilityTime;
    }

    private void Die()
    {
        Debug.Log("[PlayerHealth] Player died!");

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ResetRun();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void SetColor(Color color)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }

    private void OnStatsUpdated()
    {
        float newMax = MaxHealth;

        // ≈сли максимум вырос Ч лечим на разницу
        if (newMax > previousMaxHealth)
        {
            float difference = newMax - previousMaxHealth;
            currentHealth += difference;
        }

        // ≈сли максимум уменьшилс€ Ч ограничиваем
        if (currentHealth > newMax)
            currentHealth = newMax;

        previousMaxHealth = newMax;

        OnHealthChanged?.Invoke(currentHealth, newMax);
    }
}