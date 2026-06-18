using UnityEngine;
using System.Collections.Generic;

public enum ItemType
{
    StatBoost,
    UniqueEffect,
    Hybrid
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

[CreateAssetMenu(fileName = "New Item", menuName = "Roguelike/Item Data")]
public class ItemDataSO : ScriptableObject
{
    [Header("Основная информация")]
    public string itemName;
    public string itemId;
    public Sprite icon;
    [TextArea(2, 4)]
    public string description;

    [Header("Классификация")]
    public ItemType itemType;
    public Rarity rarity;

    [Header("Модификаторы статов")]
    public List<StatModifier> statModifiers = new List<StatModifier>();

    [Header("Уникальные эффекты")]
    public List<ItemEffectSO> effects = new List<ItemEffectSO>();
}