using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum ItemPool
{
    Treasure,   // сокровищницы
    Boss,       // с боссов
    Shop,       // в магазине (будущее)
    Secret,     // секретные комнаты (будущее)
    Any         // любой пул
}

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Roguelike/Item Database")]
public class ItemDatabase : ScriptableObject
{
    // ==================== СИНГЛТОН ====================
    private static ItemDatabase _instance;
    public static ItemDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ItemDatabase>("ItemDatabase");
                if (_instance == null)
                    Debug.LogError("[ItemDatabase] Not found in Resources folder!");
            }
            return _instance;
        }
    }

    // ==================== ДАННЫЕ ====================

    [System.Serializable]
    public class ItemEntry
    {
        public ItemDataSO item;

        [Tooltip("Из каких источников может выпасть")]
        public List<ItemPool> pools = new List<ItemPool> { ItemPool.Treasure };

        [Range(0f, 100f)]
        [Tooltip("Вес выпадения (больше = чаще)")]
        public float weight = 10f;

        [Tooltip("Может ли выпасть повторно в одном забеге")]
        public bool canRepeat = false;

        [Tooltip("Может ли стакаться (несколько копий)")]
        public bool canStack = false;
    }

    [SerializeField] private List<ItemEntry> allItems = new List<ItemEntry>();

    // Предметы, полученные в текущем забеге
    [System.NonSerialized]
    private HashSet<ItemDataSO> acquiredThisRun = new HashSet<ItemDataSO>();

    // Сколько раз каждый предмет получен (для стакающихся)
    [System.NonSerialized]
    private Dictionary<ItemDataSO, int> itemStacks = new Dictionary<ItemDataSO, int>();

    // ==================== МЕТОДЫ ====================

    // Сбросить для нового забега
    public void ResetForNewRun()
    {
        acquiredThisRun.Clear();
        itemStacks.Clear();
        Debug.Log("[ItemDatabase] Reset for new run");
    }

    // Получить случайный предмет из указанного пула
    public ItemDataSO GetRandomItem(ItemPool pool, bool excludeAcquired = true)
    {
        var available = GetAvailableItems(pool, excludeAcquired);

        if (available.Count == 0)
        {
            Debug.LogWarning($"[ItemDatabase] No available items in pool: {pool}");
            return null;
        }

        // Взвешенный рандом
        float totalWeight = available.Sum(e => e.weight);
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var entry in available)
        {
            cumulative += entry.weight;
            if (roll <= cumulative)
                return entry.item;
        }

        return available.Last().item;
    }

    // Получить список доступных предметов
    public List<ItemEntry> GetAvailableItems(ItemPool pool, bool excludeAcquired = true)
    {
        return allItems.Where(entry =>
        {
            // Проверка пула
            if (pool != ItemPool.Any && !entry.pools.Contains(pool))
                return false;

            // Проверка на уже полученные
            if (excludeAcquired && !entry.canRepeat && acquiredThisRun.Contains(entry.item))
                return false;

            return entry.item != null;
        }).ToList();
    }

    //Пометить предмет как полученный
    public void MarkAsAcquired(ItemDataSO item)
    {
        if (item == null) return;

        acquiredThisRun.Add(item);

        if (!itemStacks.ContainsKey(item))
            itemStacks[item] = 0;
        itemStacks[item]++;

        Debug.Log($"[ItemDatabase] Acquired: {item.itemName} (x{itemStacks[item]})");
    }
   
    // Проверить, был ли предмет уже получен
    public bool WasAcquired(ItemDataSO item)
    {
        return acquiredThisRun.Contains(item);
    }

    // Сколько раз предмет был получен
    public int GetStackCount(ItemDataSO item)
    {
        return itemStacks.TryGetValue(item, out int count) ? count : 0;
    }

    // Получить все предметы в пуле (для дебага/UI)
    public List<ItemDataSO> GetAllItemsInPool(ItemPool pool)
    {
        return allItems
            .Where(e => e.pools.Contains(pool) || pool == ItemPool.Any)
            .Select(e => e.item)
            .ToList();
    }

    // Найти entry по предмету
    public ItemEntry GetEntry(ItemDataSO item)
    {
        return allItems.FirstOrDefault(e => e.item == item);
    }
}