using UnityEngine;


public class PlayerShooting : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    private PlayerStats stats;
    private Inventory inventory;
    private float fireCooldown = 0f;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        inventory = GetComponent<Inventory>();
    }

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        float fireRate = stats.GetStat(StatType.FireRate);

        if (Input.GetMouseButton(0) && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = fireRate;
        }
    }

    void Shoot()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = ((Vector2)(mouseWorld - firePoint.position)).normalized;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position,
                                           Quaternion.identity);

        // »гнорим коллизию пули с игроком
        var bulletCol = bulletObj.GetComponent<Collider2D>();
        var playerCol = GetComponent<Collider2D>();
        if (bulletCol != null && playerCol != null)
            Physics2D.IgnoreCollision(bulletCol, playerCol);

        Bullet bullet = bulletObj.GetComponent<Bullet>();

        // —читаем урон из статов
        float damage = stats.GetStat(StatType.Damage);

        //  рит
        float critChance = stats.GetStat(StatType.CritChance);
        bool isCrit = Random.value < critChance;
        if (isCrit)
        {
            float critMult = stats.GetStat(StatType.CritMultiplier);
            damage *= critMult;
        }

        float bulletSpeed = stats.GetStat(StatType.BulletSpeed);

        // ѕередаЄм пуле всЄ что нужно
        bullet.Init(direction, damage, bulletSpeed, isCrit, inventory);
    }
}
