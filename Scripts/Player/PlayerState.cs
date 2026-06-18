using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerState : MonoBehaviour
{
    public int maxHealth = 5;
    public float invincibilityTime = 1f;

    private SpriteRenderer spriteRenderer;
    private Color normalColor = Color.white;
    public Color invincibleColor = new Color(1f, 1f, 1f, 0.4f);


    private int currentHealth;
    public int CurrentHealth => currentHealth;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;


    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;
    }

    void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                SetNormalColor();
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible) return;

        currentHealth -= amount;
        Debug.Log("Player took " + amount + " damage. HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }

        isInvincible = true;
        invincibilityTimer = invincibilityTime;
        SetInvincibleColor();
    }


    void Die()
    {
        Debug.Log("Player died!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void SetInvincibleColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = invincibleColor;
    }

    void SetNormalColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;
    }

    public void SetInvincible(bool value)
    {
        isInvincible = value;

        if (value)
        {
            invincibilityTimer = invincibilityTime;
            SetInvincibleColor();
        }
        else
        {
            SetNormalColor();
        }
    }
}
