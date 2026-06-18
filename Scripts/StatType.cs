using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    MaxHealth,     // макс хп
    MoveSpeed,     // скорость
    Damage,        // урон
    FireRate,       // время между выстрелами (меньше = быстрее)
    BulletSpeed,   // скорость пуль (больше = быстрее)
    DashSpeed,    // получаемое ускорение при рывке
    DashDuration,  // длительность ускорения
    DashCooldown,   // кд дэша
    Armor,        // броня
    CritChance,   // крит шанс
    CritMultiplier,  // крит урон || 2.0x
    DodgeChance,   // шанс негации урона
    HealthRegen   // хп реген
}

public enum ModifierType
{
    Flat,       // Плоский урон || +10 к урону
    Percent     // Процентный урон || +10% к урону
}

[System.Serializable]
public class StatModifier
{
    public StatType statType;
    public ModifierType modifierType;
    public float value;
    [HideInInspector] public object source; // кто добавил (предмет, бафф)
}
