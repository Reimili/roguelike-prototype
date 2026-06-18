// ==== MinionEnemy.cs ====
using UnityEngine;

public class MinionEnemy : BaseEnemy
{
    [Header("Minion Settings")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float wobbleAmount = 20f;
    [SerializeField] private float wobbleSpeed = 3f;

    [Header("Spawn Delay")]
    [SerializeField] private float spawnDelay = 0.3f;

    private float spawnTimer;

    protected override void Start()
    {
        base.Start();
        spawnTimer = spawnDelay;
    }

    void FixedUpdate()
    {
        if (player == null || !IsAlive) return;

        // ∆дЄм пока разлЄт отработает
        if (spawnTimer > 0f)
        {
            spawnTimer -= Time.fixedDeltaTime;
            return;
        }

        FacePlayer();
        HandleContactDamage();

        Vector2 dir = DirectionToPlayer();
        float wobble = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;
        dir = Quaternion.Euler(0, 0, wobble) * dir;

        MoveTowards(dir, chaseSpeed);
    }
}