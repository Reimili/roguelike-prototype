// ==== BossEnemy.cs ====
using UnityEngine;

public class BossEnemy : BaseEnemy
{
    public event System.Action OnBossDeath;

    private string bossName;

    public override void Initialize(EnemyDataSO data, int level)
    {
        base.Initialize(data, level);

        bossName = data.enemyName;

        // Показываем полоску здоровья
        if (BossHealthBarUI.Instance != null)
        {
            BossHealthBarUI.Instance.Show(bossName, maxHealth);
        }
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);

        // Обновляем полоску
        if (BossHealthBarUI.Instance != null)
        {
            BossHealthBarUI.Instance.UpdateHealth(currentHealth);
        }
    }

    protected override void OnDeath()
    {
        Debug.Log($"[BossEnemy] {name} has been defeated!");

        // Скрываем полоску
        if (BossHealthBarUI.Instance != null)
        {
            BossHealthBarUI.Instance.Hide();
        }

        OnBossDeath?.Invoke();
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        OnDeath();

        Destroy(gameObject);
    }
}