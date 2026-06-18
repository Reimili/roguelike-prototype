// ==== NecromancerBoss.cs ====
using UnityEngine;
using System.Collections;

public class NecromancerBoss : BossEnemy
{
    [Header("Phase Settings")]
    [SerializeField] private float phase2Threshold = 0.5f; // 50% HP

    [Header("Movement")]
    [SerializeField] private float teleportCooldown = 5f;

    [Header("Shooting - Phase 1")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private float burstCooldown = 2f;
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstSpread = 15f;
    [SerializeField] private float timeBetweenShots = 0.15f;

    [Header("Shooting - Phase 2")]
    [SerializeField] private int ringBulletCount = 12;
    [SerializeField] private float ringCooldown = 3f;

    [Header("Summon")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private EnemyDataSO minionData;
    [SerializeField] private int phase1SummonCount = 2;
    [SerializeField] private int phase2SummonCount = 4;
    [SerializeField] private float summonCooldown = 6f;
    [SerializeField] private float minionScatterForce = 3f;

    [Header("Visual")]
    [SerializeField] private Color phase2Color = new Color(0.8f, 0.2f, 1f);

    // Состояния
    private enum BossPhase { Phase1, Phase2 }
    private BossPhase currentPhase = BossPhase.Phase1;

    // Таймеры
    private float burstTimer;
    private float ringTimer;
    private float summonTimer;
    private float teleportTimer;

    // Флаги
    private bool isShooting = false;
    private bool isTeleporting = false;

    protected override void Start()
    {
        base.Start();

        burstTimer = 1f;
        ringTimer = ringCooldown;
        summonTimer = summonCooldown * 0.5f;
        teleportTimer = teleportCooldown;
    }

    void FixedUpdate()
    {
        if (player == null || !IsAlive) return;
        if (isTeleporting) return;

        FacePlayer();
        CheckPhaseTransition();
        UpdateTimers();

        switch (currentPhase)
        {
            case BossPhase.Phase1:
                Phase1Behavior();
                break;

            case BossPhase.Phase2:
                Phase2Behavior();
                break;
        }
    }

    // ==================== ФАЗЫ ====================

    private void CheckPhaseTransition()
    {
        if (currentPhase == BossPhase.Phase1)
        {
            float hpPercent = currentHealth / maxHealth;

            if (hpPercent <= phase2Threshold)
            {
                EnterPhase2();
            }
        }
    }

    private void EnterPhase2()
    {
        currentPhase = BossPhase.Phase2;

        // Визуальная индикация
        if (spriteRenderer != null)
            spriteRenderer.color = phase2Color;

        // Телепортируемся в центр
        StartCoroutine(Teleport());

        // Призываем волну врагов
        SummonMinions(phase2SummonCount);

        Debug.Log("[NecromancerBoss] Phase 2 activated!");
    }

    private void Phase1Behavior()
    {
        // Стреляем очередями
        if (burstTimer <= 0f && !isShooting)
        {
            StartCoroutine(ShootBurst());
            burstTimer = burstCooldown;
        }

        // Призываем врагов
        if (summonTimer <= 0f)
        {
            SummonMinions(phase1SummonCount);
            summonTimer = summonCooldown;
        }

        // Медленно двигаемся
        SlowChase();
    }

    private void Phase2Behavior()
    {
        // Стреляем очередями (быстрее)
        if (burstTimer <= 0f && !isShooting)
        {
            StartCoroutine(ShootBurst());
            burstTimer = burstCooldown * 0.6f;
        }

        // Стреляем кольцом
        if (ringTimer <= 0f)
        {
            ShootRing();
            ringTimer = ringCooldown;
        }

        // Призываем больше врагов
        if (summonTimer <= 0f)
        {
            SummonMinions(phase2SummonCount);
            summonTimer = summonCooldown * 0.7f;
        }

        // Телепортируемся
        if (teleportTimer <= 0f && !isTeleporting)
        {
            StartCoroutine(Teleport());
            teleportTimer = teleportCooldown;
        }

        // Медленно двигаемся
        SlowChase();
    }

    // ==================== ТАЙМЕРЫ ====================

    private void UpdateTimers()
    {
        burstTimer -= Time.fixedDeltaTime;
        summonTimer -= Time.fixedDeltaTime;

        if (currentPhase == BossPhase.Phase2)
        {
            ringTimer -= Time.fixedDeltaTime;
            teleportTimer -= Time.fixedDeltaTime;
        }
    }

    // ==================== ДВИЖЕНИЕ ====================

    private void SlowChase()
    {
        if (isShooting || isTeleporting) return;

        Vector2 dir = DirectionToPlayer();
        MoveTowards(dir, moveSpeed);
    }

    // ==================== АТАКИ ====================

    private IEnumerator ShootBurst()
    {
        if (bulletPrefab == null) yield break;

        isShooting = true;
        rb.velocity = Vector2.zero;

        Vector2 baseDir = DirectionToPlayer();

        for (int i = 0; i < burstCount; i++)
        {
            if (!IsAlive) yield break;

            float spread = Random.Range(-burstSpread, burstSpread);
            Vector2 dir = Quaternion.Euler(0, 0, spread) * baseDir;

            SpawnBullet(dir);

            yield return new WaitForSeconds(timeBetweenShots);
        }

        isShooting = false;
    }

    private void ShootRing()
    {
        if (bulletPrefab == null) return;

        float angleStep = 360f / ringBulletCount;

        for (int i = 0; i < ringBulletCount; i++)
        {
            float angle = angleStep * i;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;

            SpawnBullet(dir);
        }

        Debug.Log("[NecromancerBoss] Ring shot!");
    }

    private void SpawnBullet(Vector2 direction)
    {
        GameObject bullet = Instantiate(
            bulletPrefab,
            transform.position,
            Quaternion.identity
        );

        // Игнорим коллизию с собой
        Collider2D bulletCol = bullet.GetComponent<Collider2D>();
        Collider2D myCol = GetComponent<Collider2D>();
        if (bulletCol != null && myCol != null)
            Physics2D.IgnoreCollision(bulletCol, myCol);

        EnemyBullet eb = bullet.GetComponent<EnemyBullet>();
        if (eb != null)
        {
            eb.Init(direction, damage, bulletSpeed, 5f);
        }
    }

    // ==================== ПРИЗЫВ ====================

    private void SummonMinions(int count)
    {
        if (minionPrefab == null) return;

        Debug.Log($"[NecromancerBoss] Summoning {count} minions!");

        for (int i = 0; i < count; i++)
        {
            GameObject minion = Instantiate(
                minionPrefab,
                transform.position,
                Quaternion.identity
            );

            // Инициализируем
            if (minionData != null)
            {
                BaseEnemy enemy = minion.GetComponent<BaseEnemy>();
                if (enemy != null)
                {
                    enemy.Initialize(minionData, 1);
                }
            }

            // Разлёт в стороны
            Rigidbody2D minionRb = minion.GetComponent<Rigidbody2D>();
            if (minionRb != null)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                minionRb.AddForce(randomDir * minionScatterForce, ForceMode2D.Impulse);
            }
        }
    }

    // ==================== ТЕЛЕПОРТ ====================

    private IEnumerator Teleport()
    {
        isTeleporting = true;

        // Мигание перед телепортом
        for (int i = 0; i < 5; i++)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
        }

        // Ищем валидную позицию внутри арены
        Vector2 newPos = FindValidTeleportPosition();
        transform.position = newPos;

        // Показываем спрайт
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        isTeleporting = false;
    }

    private Vector2 FindValidTeleportPosition()
    {
        // Получаем границы арены
        Vector2 arenaCenter = GetArenaCenter();
        Vector2 arenaHalfSize = GetArenaHalfSize();

        // Отступ от стен чтобы босс не застревал
        float padding = 1.5f;

        float minX = arenaCenter.x - arenaHalfSize.x + padding;
        float maxX = arenaCenter.x + arenaHalfSize.x - padding;
        float minY = arenaCenter.y - arenaHalfSize.y + padding;
        float maxY = arenaCenter.y + arenaHalfSize.y - padding;

        // Пробуем найти точку далеко от игрока
        int attempts = 20;

        for (int i = 0; i < attempts; i++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );

            // Проверяем что точка достаточно далеко от игрока
            if (player != null)
            {
                float distToPlayer = Vector2.Distance(candidate, player.position);
                if (distToPlayer > 3f)
                    return candidate;
            }
            else
            {
                return candidate;
            }
        }

        // Если не нашли идеальную точку — просто центр арены
        return arenaCenter;
    }

    private Vector2 GetArenaCenter()
    {
        // Арена создаётся на месте комнаты босса
        Transform arena = transform.parent;
        if (arena != null)
            return arena.position;

        return transform.position;
    }

    private Vector2 GetArenaHalfSize()
    {
        // Половина размера арены
        // У тебя арена 24x16, значит половина 12x8
        return new Vector2(12f, 8f);
    }

    // ==================== СМЕРТЬ ====================

    protected override void OnDeath()
    {
        // Останавливаем все корутины
        StopAllCoroutines();

        base.OnDeath();
    }
}