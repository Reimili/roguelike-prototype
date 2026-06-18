// ==== CowardEnemy.cs ====
using UnityEngine;
using System.Collections;

public class CowardEnemy : BaseEnemy
{
    [Header("Coward Settings")]
    [SerializeField] private float fleeSpeed = 4f;
    [SerializeField] private float panicDistance = 5f;

    [Header("Zigzag")]
    [SerializeField] private float zigzagInterval = 0.5f;
    [SerializeField] private float zigzagAngle = 45f;

    [Header("Spawn On Death")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private EnemyDataSO minionData;
    [SerializeField] private int minionCount = 3;
    [SerializeField] private float scatterForce = 3f;

    private float zigzagTimer = 0f;
    private Vector2 currentFleeDir;

    protected override void Start()
    {
        base.Start();

        // Носитель НЕ наносит контактный урон
        contactDamage = 0f;
        damage = 0f;
    }

    void FixedUpdate()
    {
        if (player == null || !IsAlive) return;

        FacePlayer();

        float dist = DistanceToPlayer();

        zigzagTimer -= Time.fixedDeltaTime;
        if (zigzagTimer <= 0f)
        {
            UpdateFleeDirection();
            zigzagTimer = zigzagInterval;
        }

        float currentSpeed = dist < panicDistance ? fleeSpeed * 1.3f : fleeSpeed;
        MoveTowards(currentFleeDir, currentSpeed);
    }

    private void UpdateFleeDirection()
    {
        Vector2 awayFromPlayer = DirectionFromPlayer();

        float angle = Random.Range(-zigzagAngle, zigzagAngle);
        currentFleeDir = Quaternion.Euler(0, 0, angle) * awayFromPlayer;
    }

    // Полностью отключаем контактный урон для носителя
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // Ничего не делаем — носитель не атакует
    }

    protected override void OnDeath()
    {
        SpawnMinions();
    }

    private void SpawnMinions()
    {
        if (minionPrefab == null)
        {
            Debug.LogWarning($"[{name}] No minion prefab assigned!");
            return;
        }

        Debug.Log($"[{name}] Spawning {minionCount} minions!");

        for (int i = 0; i < minionCount; i++)
        {
            // Все спавнятся в одной точке — в позиции носителя
            Vector3 spawnPos = transform.position;

            GameObject minion = Instantiate(minionPrefab, spawnPos, Quaternion.identity);

            // Добавляем в комнату
            if (room != null)
            {
                minion.transform.SetParent(room.transform);
                room.RegisterEnemy(minion);
            }

            // Инициализируем из SO
            if (minionData != null)
            {
                BaseEnemy enemyScript = minion.GetComponent<BaseEnemy>();
                if (enemyScript != null)
                {
                    int level = room != null ? room.currentLevel : 1;
                    enemyScript.Initialize(minionData, level);
                }
            }

            // Импульс в случайную сторону
            Rigidbody2D minionRb = minion.GetComponent<Rigidbody2D>();
            if (minionRb != null)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                minionRb.AddForce(randomDir * scatterForce, ForceMode2D.Impulse);
            }
        }
    }
}
