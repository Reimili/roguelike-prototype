// ==== WorldItem.cs ====
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class WorldItem : MonoBehaviour
{
    [SerializeField] private ItemDataSO itemData;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.15f;

    [Header("Rarity Glow (optional)")]
    [SerializeField] private bool useRarityGlow = false;
    [SerializeField] private SpriteRenderer glowRenderer;  // Отдельный спрайт для свечения

    private SpriteRenderer spriteRenderer;
    private Vector3 startPos;

    public void Initialize(ItemDataSO data)
    {
        itemData = data;
        Setup();
    }

    void Start()
    {
        if (itemData != null)
            Setup();
    }

    void Setup()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Иконка — ВСЕГДА белый цвет, чтобы спрайт отображался нормально!
        spriteRenderer.sprite = itemData.icon;
        spriteRenderer.color = Color.white;  // ← ВАЖНО!

        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        startPos = transform.position;

        // Если хочешь свечение — используй отдельный спрайт
        if (useRarityGlow && glowRenderer != null)
        {
            glowRenderer.color = GetRarityColor(itemData.rarity);
        }

        Debug.Log($"[WorldItem] Setup: {itemData.itemName}, Rarity: {itemData.rarity}");
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var inventory = other.GetComponent<Inventory>();
        if (inventory != null && inventory.TryAddItem(itemData))
        {
            Debug.Log($"[WorldItem] Picked up: {itemData.itemName}");
            Destroy(gameObject);
        }
    }

    private Color GetRarityColor(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => Color.white,
            Rarity.Uncommon => Color.green,
            Rarity.Rare => Color.blue,
            Rarity.Legendary => Color.yellow,
            _ => Color.white
        };
    }
}