// ==== BomberEnemy.cs ====
using UnityEngine;
using System.Collections;

public class BomberEnemy : BaseEnemy
{
    [Header("Bomber Settings")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float acceleration = 0.5f;

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float explosionDamage = 3f;
    [SerializeField] private bool damagesEnemies = true;
    [SerializeField] private float detonationRange = 0.8f;

    [Header("Visual")]
    [SerializeField] private float flashSpeedNormal = 2f;
    [SerializeField] private float flashSpeedClose = 8f;
    [SerializeField] private float flashDistance = 3f;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private GameObject explosionVFXPrefab;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float knockbackDuration = 0.25f;

    private float currentSpeed;
    private bool isExploding = false;

    protected override void Start()
    {
        base.Start();
        currentSpeed = chaseSpeed * 0.5f;
        contactDamage = 0; // Не наносит контактный урон, только взрыв
    }

    void FixedUpdate()
    {
        if (player == null || !IsAlive || isExploding) return;

        FacePlayer();

        float dist = DistanceToPlayer();

        // Ускоряемся со временем
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.fixedDeltaTime, chaseSpeed);

        // Бежим к игроку
        Vector2 dir = DirectionToPlayer();
        MoveTowards(dir, currentSpeed);

        // Мигание — быстрее когда ближе
        float flashSpeed = dist < flashDistance ? flashSpeedClose : flashSpeedNormal;
        float flash = Mathf.PingPong(Time.time * flashSpeed, 1f);
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(Color.white, flashColor, flash);
        }

        // Взрываемся при контакте
        if (dist <= detonationRange)
        {
            Explode();
        }
    }

    protected override void OnDeath()
    {
        // Взрыв при смерти
        if (!isExploding)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (isExploding) return;
        isExploding = true;

        Debug.Log($"[{name}] BOOM! Radius: {explosionRadius}, Damage: {explosionDamage}");

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            // Урон игроку
            if (hit.CompareTag("Player"))
            {
                PlayerHealth ph = hit.GetComponent<PlayerHealth>();
                if (ph != null)
                    ph.TakeDamage(explosionDamage);
            }

            // Урон другим врагам
            if (damagesEnemies)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable != null && hit.gameObject != gameObject)
                    damageable.TakeDamage(explosionDamage);
            }

            // Отталкивание всех в радиусе
            Knockback kb = hit.GetComponent<Knockback>();
            if (kb != null)
            {
                kb.ApplyWithFalloff(
                    transform.position,
                    knockbackForce,
                    explosionRadius,
                    knockbackDuration
                );
            }
        }

        SpawnExplosionVFX();

        if (!isDead)
        {
            isDead = true;
            if (room != null)
                room.EnemyDefeated(gameObject);
            Destroy(gameObject);
        }
    }

    private void SpawnExplosionVFX()
    {
        if (explosionVFXPrefab == null)
            return;

        GameObject vfxObj = Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);

        ExplosionVFX vfx = vfxObj.GetComponent<ExplosionVFX>();
        if (vfx != null)
        {
            vfx.Play(explosionRadius);
        }
        else
        {
            // fallback, если скрипт не найден
            vfxObj.transform.localScale = Vector3.one * explosionRadius * 2f;
            Destroy(vfxObj, 0.5f);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Показываем радиус взрыва в редакторе
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, explosionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detonationRange);
    }
}