// ==== MeleeEnemy.cs ====
using UnityEngine;
using System.Collections;

public class MeleeEnemy : BaseEnemy
{
    [Header("Melee Settings")]
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private float chaseSpeed = 3f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 8f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldownMin = 2f;
    [SerializeField] private float dashCooldownMax = 4f;
    [SerializeField] private float sideDashChance = 0.3f;


    private float dashTimer = 0f;
    private float attackTimer = 0f;
    private bool isDashing = false;
    private Vector2 dashDirection;

    private enum MeleeState { Chase, Attack, Dash }
    private MeleeState state = MeleeState.Chase;

    protected override void Start()
    {
        base.Start();
        dashTimer = Random.Range(1f, dashCooldownMax);
    }

    void FixedUpdate()
    {
        if (player == null || !IsAlive || isDashing) return;

        FacePlayer();
        HandleContactDamage();

        float dist = DistanceToPlayer();

        // Таймеры
        dashTimer -= Time.fixedDeltaTime;
        attackTimer -= Time.fixedDeltaTime;

        // Решаем что делать
        if (dashTimer <= 0f && dist > attackRange)
        {
            StartCoroutine(PerformDash());
            return;
        }

        if (dist <= attackRange)
        {
            state = MeleeState.Attack;
        }
        else
        {
            state = MeleeState.Chase;
        }

        switch (state)
        {
            case MeleeState.Chase:
                ChasePlayer();
                break;

            case MeleeState.Attack:
                AttackPlayer();
                break;
        }
    }

    private void ChasePlayer()
    {
        Vector2 dir = DirectionToPlayer();
        MoveTowards(dir, chaseSpeed);
    }

    private void AttackPlayer()
    {
        // Стоим и наносим урон через контакт
        rb.velocity = Vector2.zero;
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        dashTimer = Random.Range(dashCooldownMin, dashCooldownMax);

        // Выбираем направление рывка
        if (Random.value < sideDashChance)
        {
            // Рывок вбок
            Vector2 toPlayer = DirectionToPlayer();
            Vector2 perpendicular = new Vector2(-toPlayer.y, toPlayer.x);
            dashDirection = Random.value < 0.5f ? perpendicular : -perpendicular;
        }
        else
        {
            // Рывок к игроку
            dashDirection = DirectionToPlayer();
        }

        // Выполняем рывок
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            if (!IsAlive) yield break;

            rb.MovePosition(rb.position + dashDirection * (dashSpeed * Time.fixedDeltaTime));
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;
    }
}