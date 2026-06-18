// ==== LevelSettings.cs ====
using UnityEngine;

[System.Serializable]
public class LevelSettings
{
    public int level = 1;

    [Header("Бюджет комнаты")]
    [Tooltip("Минимальный бюджет очков на комнату")]
    public int roomBudgetMin = 5;
    [Tooltip("Максимальный бюджет очков на комнату")]
    public int roomBudgetMax = 10;

    [Header("Лимиты")]
    [Tooltip("Максимум врагов в одной комнате")]
    public int maxEnemiesPerRoom = 6;

    [Header("Шансы")]
    [Range(0f, 1f)]
    [Tooltip("Шанс появления элитного врага в комнате")]
    public float eliteChance = 0.1f;
}