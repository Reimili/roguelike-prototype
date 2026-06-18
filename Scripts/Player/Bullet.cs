using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;

    private Rigidbody2D rb;
    private float damage;
    private bool isCrit;
    private Inventory ownerInventory;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// »нициализаци€ пули Ч вызываетс€ из PlayerShooting
    /// </summary>
    public void Init(Vector2 direction, float damage, float speed,
                     bool isCrit, Inventory inventory)
    {
        this.damage = damage;
        this.isCrit = isCrit;
        this.ownerInventory = inventory;

        rb.velocity = direction.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    // ƒл€ обратной совместимости (если где-то ещЄ используетс€)
    public void SetDirection(Vector2 direction)
    {
        rb.velocity = direction.normalized * 10f;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // ”ниверсальный урон через интерфейс
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            float finalDamage = damage;

            // Ёффекты предметов (поджог, вампиризм и т.д.)
            if (ownerInventory != null)
                ownerInventory.NotifyDealDamage(collision.gameObject, ref finalDamage);

            damageable.TakeDamage(finalDamage);

            if (isCrit)
                Debug.Log($"CRIT! {finalDamage} damage!");

            // ≈сли враг убит Ч уведомл€ем предметы
            if (!damageable.IsAlive && ownerInventory != null)
                ownerInventory.NotifyKillEnemy(collision.gameObject);
        }

        Destroy(gameObject);
    }
}
