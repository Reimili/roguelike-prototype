// ==== EnemyBullet.cs ====
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private Rigidbody2D rb;
    private float damage;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 direction, float damage, float speed, float lifetime)
    {
        this.damage = damage;
        rb.velocity = direction.normalized * speed;

        // Поворачиваем спрайт по направлению
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }
            Destroy(gameObject);
        }

        // Уничтожаем при попадании в стены
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}