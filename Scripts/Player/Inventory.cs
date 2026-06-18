// ==== Inventory.cs ====
using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [SerializeField] private int maxSlots = 20;

    private List<ItemDataSO> items = new List<ItemDataSO>();
    private List<ItemEffectSO> activeEffects = new List<ItemEffectSO>();
    private PlayerStats stats;

    public IReadOnlyList<ItemDataSO> Items => items;
    public int ItemCount => items.Count;

    public event System.Action<ItemDataSO> OnItemAdded;
    public event System.Action<ItemDataSO> OnItemRemoved;
    public event System.Action OnInventoryChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[Inventory] Instance created");
        }
        else
        {
            Debug.LogWarning("[Inventory] Duplicate instance destroyed");
            Destroy(this);
            return;
        }

        stats = GetComponent<PlayerStats>();

        if (stats == null)
        {
            Debug.LogError("[Inventory] PlayerStats component not found!");
        }
    }

    void Update()
    {
        // Тикаем эффекты
        for (int i = 0; i < activeEffects.Count; i++)
        {
            if (activeEffects[i] != null)
            {
                activeEffects[i].OnUpdate(gameObject);
            }
        }
    }

    /// <summary>
    /// Попытаться добавить предмет в инвентарь
    /// </summary>
    public bool TryAddItem(ItemDataSO item)
    {
        if (item == null)
        {
            Debug.LogWarning("[Inventory] Trying to add null item!");
            return false;
        }

        if (items.Count >= maxSlots)
        {
            Debug.Log("[Inventory] Inventory is full!");
            return false;
        }

        // Проверяем можно ли стакать
        var entry = ItemDatabase.Instance?.GetEntry(item);
        bool canStack = entry?.canStack ?? false;

        if (!canStack && items.Contains(item))
        {
            Debug.Log($"[Inventory] Already have {item.itemName} and it can't stack!");
            return false;
        }

        // Добавляем предмет
        items.Add(item);

        // Уведомляем базу данных
        if (ItemDatabase.Instance != null)
        {
            ItemDatabase.Instance.MarkAsAcquired(item);
        }

        // Применяем модификаторы статов
        if (stats != null && item.statModifiers != null)
        {
            foreach (var mod in item.statModifiers)
            {
                mod.source = item;
                stats.AddModifier(mod);
            }
        }

        // Активируем эффекты
        if (item.effects != null)
        {
            foreach (var effect in item.effects)
            {
                if (effect != null)
                {
                    activeEffects.Add(effect);
                    effect.OnPickup(gameObject);
                }
            }
        }

        Debug.Log($"[Inventory] Added: {item.itemName}. Total items: {items.Count}");

        OnItemAdded?.Invoke(item);
        OnInventoryChanged?.Invoke();

        return true;
    }

    /// <summary>
    /// Удалить предмет из инвентаря
    /// </summary>
    public void RemoveItem(ItemDataSO item)
    {
        if (item == null) return;

        if (!items.Remove(item))
        {
            Debug.LogWarning($"[Inventory] Item {item.itemName} not found in inventory!");
            return;
        }

        // Убираем модификаторы
        if (stats != null)
        {
            stats.RemoveModifiersFromSource(item);
        }

        // Деактивируем эффекты
        if (item.effects != null)
        {
            foreach (var effect in item.effects)
            {
                if (effect != null)
                {
                    effect.OnRemove(gameObject);
                    activeEffects.Remove(effect);
                }
            }
        }

        Debug.Log($"[Inventory] Removed: {item.itemName}. Total items: {items.Count}");

        OnItemRemoved?.Invoke(item);
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Очистить инвентарь полностью
    /// </summary>
    public void Clear()
    {
        while (items.Count > 0)
        {
            RemoveItem(items[0]);
        }

        Debug.Log("[Inventory] Cleared");
    }

    /// <summary>
    /// Проверить есть ли предмет в инвентаре
    /// </summary>
    public bool HasItem(ItemDataSO item)
    {
        return items.Contains(item);
    }

    /// <summary>
    /// Посчитать сколько копий предмета в инвентаре
    /// </summary>
    public int CountItem(ItemDataSO item)
    {
        int count = 0;
        foreach (var i in items)
        {
            if (i == item)
                count++;
        }
        return count;
    }

    /// <summary>
    /// Получить предмет по индексу
    /// </summary>
    public ItemDataSO GetItem(int index)
    {
        if (index >= 0 && index < items.Count)
            return items[index];
        return null;
    }

    // ==================== ХУКИ ДЛЯ БОЕВОЙ СИСТЕМЫ ====================

    /// <summary>
    /// Вызывается когда игрок наносит урон
    /// </summary>
    public void NotifyDealDamage(GameObject enemy, ref float damage)
    {
        for (int i = 0; i < activeEffects.Count; i++)
        {
            if (activeEffects[i] != null)
            {
                activeEffects[i].OnDealDamage(gameObject, enemy, ref damage);
            }
        }
    }

    /// <summary>
    /// Вызывается когда игрок получает урон
    /// </summary>
    public void NotifyTakeDamage(ref float damage)
    {
        for (int i = 0; i < activeEffects.Count; i++)
        {
            if (activeEffects[i] != null)
            {
                activeEffects[i].OnTakeDamage(gameObject, ref damage);
            }
        }
    }

    /// <summary>
    /// Вызывается когда игрок убивает врага
    /// </summary>
    public void NotifyKillEnemy(GameObject enemy)
    {
        for (int i = 0; i < activeEffects.Count; i++)
        {
            if (activeEffects[i] != null)
            {
                activeEffects[i].OnKillEnemy(gameObject, enemy);
            }
        }
    }
}