// ==== StatRowUI.cs ====
using UnityEngine;
using TMPro;

public class StatRowUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI valueText;

    public TextMeshProUGUI ValueText => valueText;

    public void Setup(StatType type)
    {
        if (nameText != null)
        {
            // Заменяем пробел на неразрывный пробел
            string displayName = GetStatDisplayName(type);
            displayName = displayName.Replace(" ", "\u00A0");  // ← неразрывный пробел
            nameText.text = displayName;
        }
    }

    private string GetStatDisplayName(StatType type)
    {
        return type switch
        {
            StatType.MaxHealth => "Max Health",
            StatType.MoveSpeed => "Move Speed",
            StatType.Damage => "Damage",
            StatType.FireRate => "Fire Rate",
            StatType.BulletSpeed => "Bullet Speed",
            StatType.DashCooldown => "Dash CD",
            StatType.Armor => "Armor",
            StatType.CritChance => "Crit Chance",
            StatType.CritMultiplier => "Crit Mult",
            StatType.DodgeChance => "Dodge",
            StatType.HealthRegen => "HP Regen",
            _ => type.ToString()
        };
    }
}