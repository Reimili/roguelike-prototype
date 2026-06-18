// ==== EnemyDataSO.cs ==== (со статами)
using UnityEngine;

public enum EnemyCategory
{
    Minion,
    Regular,
    Elite,
    MiniBoss,
    Boss
}

[CreateAssetMenu(fileName = "New Enemy", menuName = "Roguelike/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [Header("Основная информация")]
    public string enemyName;
    public GameObject prefab;
    public Sprite sprite;          // Спрайт врага (разный вид)

    [Header("Классификация")]
    public EnemyCategory category;
    public int minLevel = 1;
    public int maxLevel = 99;

    [Header("Статы")]
    public float health = 5f;
    public float damage = 1f;
    public float moveSpeed = 2f;

    [Header("Спавн")]
    public int spawnCost = 2;
    public int maxPerRoom = 5;
    [Range(0f, 100f)]
    public float spawnWeight = 10f;

    [Header("Масштабирование по уровню")]
    [Tooltip("Множитель HP за каждый уровень")]
    public float healthPerLevel = 1f;
    [Tooltip("Множитель урона за каждый уровень")]
    public float damagePerLevel = 0.5f;

    /// <summary>
    /// Получить HP для конкретного уровня
    /// </summary>
    public float GetHealth(int level)
    {
        return health + healthPerLevel * (level - 1);
    }

    /// <summary>
    /// Получить урон для конкретного уровня
    /// </summary>
    public float GetDamage(int level)
    {
        return damage + damagePerLevel * (level - 1);
    }
}