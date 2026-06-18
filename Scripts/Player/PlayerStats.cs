// ==== PlayerStats.cs ====
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Базовые значения")]
    [SerializeField] private float baseMaxHealth = 5f;
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseDamage = 1f;
    [SerializeField] private float baseFireRate = 0.25f;
    [SerializeField] private float baseBulletSpeed = 10f;
    [SerializeField] private float baseDashSpeed = 15f;
    [SerializeField] private float baseDashDuration = 0.15f;
    [SerializeField] private float baseDashCooldown = 1f;
    [SerializeField] private float baseArmor = 0f;
    [SerializeField] private float baseCritChance = 0f;
    [SerializeField] private float baseCritMultiplier = 2f;
    [SerializeField] private float baseDodgeChance = 0f;
    [SerializeField] private float baseHealthRegen = 0f;

    private Dictionary<StatType, float> baseValues;
    private List<StatModifier> modifiers = new List<StatModifier>();

    public event System.Action OnStatsChanged;

    private void Awake()
    {
        // Синглтон
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[PlayerStats] Instance created");
        }
        else
        {
            Debug.LogWarning("[PlayerStats] Duplicate instance destroyed");
            Destroy(gameObject);
            return;
        }

        InitBaseValues();
    }

    private void InitBaseValues()
    {
        baseValues = new Dictionary<StatType, float>
        {
            { StatType.MaxHealth,       baseMaxHealth },
            { StatType.MoveSpeed,       baseMoveSpeed },
            { StatType.Damage,          baseDamage },
            { StatType.FireRate,        baseFireRate },
            { StatType.BulletSpeed,     baseBulletSpeed },
            { StatType.DashSpeed,       baseDashSpeed },
            { StatType.DashDuration,    baseDashDuration },
            { StatType.DashCooldown,    baseDashCooldown },
            { StatType.Armor,           baseArmor },
            { StatType.CritChance,      baseCritChance },
            { StatType.CritMultiplier,  baseCritMultiplier },
            { StatType.DodgeChance,     baseDodgeChance },
            { StatType.HealthRegen,     baseHealthRegen },
        };
    }

    public float GetStat(StatType type)
    {
        if (!baseValues.ContainsKey(type)) return 0f;

        float baseVal = baseValues[type];
        float flatSum = 0f;
        float percentSum = 0f;

        foreach (var mod in modifiers)
        {
            if (mod.statType != type) continue;

            if (mod.modifierType == ModifierType.Flat)
                flatSum += mod.value;
            else
                percentSum += mod.value;
        }

        return (baseVal + flatSum) * (1f + percentSum);
    }

    public void AddModifier(StatModifier mod)
    {
        modifiers.Add(mod);
        OnStatsChanged?.Invoke();
    }

    public void AddModifiers(List<StatModifier> mods)
    {
        modifiers.AddRange(mods);
        OnStatsChanged?.Invoke();
    }

    public void RemoveModifiersFromSource(object source)
    {
        modifiers.RemoveAll(m => m.source == source);
        OnStatsChanged?.Invoke();
    }

    public void RemoveModifiers(List<StatModifier> mods)
    {
        foreach (var mod in mods)
            modifiers.Remove(mod);
        OnStatsChanged?.Invoke();
    }
}