// ==== RangedEnemy.cs ====
using UnityEngine;

public class RangedEnemy : BaseEnemy
{
    [Header("Ranged Settings")]
    [SerializeField] private float tooCloseDistance = 3f;
    [SerializeField] private float tooFarDistance = 7f;
    [SerializeField] private float retreatSpeed = 3f;
    [SerializeField] private float approachSpeed = 2f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 7f;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float bulletLifetime = 4f;

    [Header("Accuracy")]
    [SerializeField] private float aimSpread = 5f;  // Градусы разброса

    private float fireTimer = 0f;

    private enum RangedState { Retreat, Hold, Approach, Shoot }
    private RangedState state = RangedState.Hold;

    protected override void Start()
    {
        base.Start();
        fireTimer = Random.Range(0f, fireRate * 0.5f);  // Не стреляют все сразу
    }

    void FixedUpdate()
    {
        if (player == null || !IsAlive) return;

        FacePlayer();

        float dist = DistanceToPlayer();
        fireTimer -= Time.fixedDeltaTime;

        // Определяем состояние
        if (dist < tooCloseDistance)
        {
            state = RangedState.Retreat;
        }
        else if (dist > tooFarDistance)
        {
            state = RangedState.Approach;
        }
        else
        {
            state = RangedState.Hold;
        }

        // Действуем
        switch (state)
        {
            case RangedState.Retreat:
                // Убегаем от игрока
                Vector2 awayDir = DirectionFromPlayer();
                MoveTowards(awayDir, retreatSpeed);
                break;

            case RangedState.Approach:
                // Подходим ближе
                Vector2 toDir = DirectionToPlayer();
                MoveTowards(toDir, approachSpeed);
                break;

            case RangedState.Hold:
                // Стоим на месте, слегка стрейфим
                rb.velocity = Vector2.zero;
                break;
        }

        // Стреляем если готовы
        if (fireTimer <= 0f && dist <= tooFarDistance)
        {
            Shoot();
            fireTimer = fireRate;
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning($"[{name}] No bullet prefab!");
            return;
        }

        Vector2 direction = DirectionToPlayer();

        // Добавляем разброс
        float spreadAngle = Random.Range(-aimSpread, aimSpread);
        direction = Quaternion.Euler(0, 0, spreadAngle) * direction;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // Игнорим коллизию с собой и другими врагами
        Collider2D bulletCol = bullet.GetComponent<Collider2D>();
        Collider2D myCol = GetComponent<Collider2D>();
        if (bulletCol != null && myCol != null)
            Physics2D.IgnoreCollision(bulletCol, myCol);

        // Настраиваем пулю
        EnemyBullet enemyBullet = bullet.GetComponent<EnemyBullet>();
        if (enemyBullet != null)
        {
            enemyBullet.Init(direction, damage, bulletSpeed, bulletLifetime);
        }
    }
}