using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerStats stats;
    private PlayerHealth playerHealth;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;
    private Knockback knockback;
    private bool isFrozen = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
        playerHealth = GetComponent<PlayerHealth>();
        knockback = GetComponent<Knockback>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isFrozen) return;

        if (!isDashing)
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
            moveInput.Normalize();
            // Поворот спрайта
            FlipSprite();
        }

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        float dashCooldown = stats.GetStat(StatType.DashCooldown);
        if (Input.GetKeyDown(KeyCode.Space)
            && dashCooldownTimer <= 0f
            && moveInput != Vector2.zero)
        {
            StartCoroutine(PerformDash());
        }
    }

    void FixedUpdate()
    {
        if (isFrozen) return;
        if (knockback != null && knockback.IsKnockedBack) return;
        if (!isDashing)
        {
            float speed = stats.GetStat(StatType.MoveSpeed);
            rb.velocity = moveInput * speed;
        }
    }

    IEnumerator PerformDash()
    {
        isDashing = true;
        dashCooldownTimer = stats.GetStat(StatType.DashCooldown);

        if (playerHealth != null)
            playerHealth.SetInvincible(true);

        // Отключаем столкновения с врагами и их пулями
        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int bulletsLayer = LayerMask.NameToLayer("Bullets");

        if (enemyLayer != -1)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        if (bulletsLayer != -1)
            Physics2D.IgnoreLayerCollision(playerLayer, bulletsLayer, true);

        float dashSpeed = stats.GetStat(StatType.DashSpeed);
        rb.velocity = moveInput * dashSpeed;

        float dashDuration = stats.GetStat(StatType.DashDuration);
        yield return new WaitForSeconds(dashDuration);

        rb.velocity = Vector2.zero;
        isDashing = false;

        if (playerHealth != null)
            playerHealth.SetInvincible(false);

        if (enemyLayer != -1)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

        if (bulletsLayer != -1)
            Physics2D.IgnoreLayerCollision(playerLayer, bulletsLayer, false);
    }
    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;
        if (frozen)
        {
            rb.velocity = Vector2.zero;
            moveInput = Vector2.zero;
        }
    }

    private void FlipSprite()
    {
        if (spriteRenderer == null) return;

        if (moveInput.x > 0)
            spriteRenderer.flipX = false;
        else if (moveInput.x < 0)
            spriteRenderer.flipX = true;
    }
}
