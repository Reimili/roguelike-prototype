// ==== EnemyDatabase.cs ====
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Roguelike/Enemy Database")]
public class EnemyDatabase : ScriptableObject
{
    private static EnemyDatabase _instance;
    public static EnemyDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<EnemyDatabase>("EnemyDatabase");
                if (_instance == null)
                    Debug.LogError("[EnemyDatabase] Not found in Resources folder!");
            }
            return _instance;
        }
    }

    [System.Serializable]
    public class EnemyEntry
    {
        public EnemyDataSO enemyData;
        [Tooltip("На каких уровнях может появляться")]
        public List<int> levels = new List<int> { 1 };
        [Tooltip("Активен ли враг (для быстрого вкл/выкл)")]
        public bool isEnabled = true;
    }

    [SerializeField] private List<EnemyEntry> allEnemies = new List<EnemyEntry>();

    [SerializeField] private List<LevelSettings> levelSettings = new List<LevelSettings>();

    // ==================== ПОЛУЧЕНИЕ ВРАГОВ ====================

    /// <summary>
    /// Получить всех врагов доступных на данном уровне
    /// </summary>
    public List<EnemyDataSO> GetEnemiesForLevel(int level, EnemyCategory? category = null)
    {
        return allEnemies
            .Where(e => e.isEnabled
                && e.enemyData != null
                && e.levels.Contains(level)
                && e.enemyData.minLevel <= level
                && e.enemyData.maxLevel >= level
                && (category == null || e.enemyData.category == category))
            .Select(e => e.enemyData)
            .ToList();
    }

    /// <summary>
    /// Получить случайного врага для уровня
    /// </summary>
    public EnemyDataSO GetRandomEnemy(int level, EnemyCategory? category = null)
    {
        var available = GetEnemiesForLevel(level, category);

        if (available.Count == 0)
        {
            Debug.LogWarning($"[EnemyDatabase] No enemies for level {level}, category {category}");
            return null;
        }

        // Взвешенный рандом
        float totalWeight = available.Sum(e => e.spawnWeight);
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var enemy in available)
        {
            cumulative += enemy.spawnWeight;
            if (roll <= cumulative)
                return enemy;
        }

        return available.Last();
    }

    /// <summary>
    /// Получить случайного босса для уровня
    /// </summary>
    public EnemyDataSO GetBossForLevel(int level)
    {
        return GetRandomEnemy(level, EnemyCategory.Boss);
    }

    /// <summary>
    /// Получить настройки для уровня
    /// </summary>
    public LevelSettings GetLevelSettings(int level)
    {
        var settings = levelSettings.FirstOrDefault(s => s.level == level);

        if (settings == null)
        {
            // Возвращаем дефолтные настройки
            Debug.LogWarning($"[EnemyDatabase] No settings for level {level}, using defaults");
            return new LevelSettings
            {
                level = level,
                roomBudgetMin = 5,
                roomBudgetMax = 10,
                maxEnemiesPerRoom = 6,
                eliteChance = 0.1f
            };
        }

        return settings;
    }

    /// <summary>
    /// Собрать набор врагов для комнаты по бюджету
    /// </summary>
    public List<EnemyDataSO> GenerateRoomEnemies(int level, RoomType roomType)
    {
        List<EnemyDataSO> result = new List<EnemyDataSO>();

        // Комната босса
        if (roomType == RoomType.Boss)
        {
            var boss = GetBossForLevel(level);
            if (boss != null)
                result.Add(boss);
            return result;
        }

        // Сокровищница — без врагов
        if (roomType == RoomType.Treasure)
            return result;

        // Обычная комната — собираем по бюджету
        var settings = GetLevelSettings(level);
        int budget = Random.Range(settings.roomBudgetMin, settings.roomBudgetMax + 1);
        int maxEnemies = settings.maxEnemiesPerRoom;

        // Доступные враги (без боссов)
        var availableMinions = GetEnemiesForLevel(level, EnemyCategory.Minion);
        var availableRegular = GetEnemiesForLevel(level, EnemyCategory.Regular);
        var availableElite = GetEnemiesForLevel(level, EnemyCategory.Elite);

        // Шанс на элитного врага
        bool hasElite = availableElite.Count > 0 && Random.value < settings.eliteChance;

        if (hasElite)
        {
            var elite = GetRandomFromList(availableElite);
            if (elite != null)
            {
                result.Add(elite);
                budget -= elite.spawnCost;
            }
        }

        // Заполняем оставшийся бюджет
        int attempts = 0;
        int maxAttempts = 50;

        while (budget > 0 && result.Count < maxEnemies && attempts < maxAttempts)
        {
            attempts++;

            // Выбираем категорию
            EnemyDataSO enemy;

            if (budget >= 2 && availableRegular.Count > 0 && Random.value < 0.5f)
            {
                enemy = GetRandomFromList(availableRegular);
            }
            else if (availableMinions.Count > 0)
            {
                enemy = GetRandomFromList(availableMinions);
            }
            else if (availableRegular.Count > 0)
            {
                enemy = GetRandomFromList(availableRegular);
            }
            else
            {
                break;
            }

            if (enemy == null || enemy.spawnCost > budget)
                continue;

            // Проверяем лимит на комнату
            int currentCount = result.Count(e => e == enemy);
            if (currentCount >= enemy.maxPerRoom)
                continue;

            result.Add(enemy);
            budget -= enemy.spawnCost;
        }

        Debug.Log($"[EnemyDatabase] Generated {result.Count} enemies for level {level} room");
        return result;
    }

    private EnemyDataSO GetRandomFromList(List<EnemyDataSO> list)
    {
        if (list == null || list.Count == 0) return null;

        float totalWeight = list.Sum(e => e.spawnWeight);
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var enemy in list)
        {
            cumulative += enemy.spawnWeight;
            if (roll <= cumulative)
                return enemy;
        }

        return list.Last();
    }
}