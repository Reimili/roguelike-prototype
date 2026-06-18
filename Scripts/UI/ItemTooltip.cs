// ==== ItemTooltip.cs ====
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Image rarityBar;

    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = Color.white;
    [SerializeField] private Color uncommonColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color rareColor = new Color(0.3f, 0.5f, 1f);
    [SerializeField] private Color legendaryColor = new Color(1f, 0.8f, 0.2f);

    [Header("Offset")]
    [SerializeField] private Vector2 offset = new Vector2(0, 80);

    private RectTransform rectTransform;
    private Canvas canvas;

    void Awake()
    {
        Instance = this;

        if (tooltipPanel == null)
            tooltipPanel = gameObject;

        rectTransform = tooltipPanel.GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        Hide();

        Debug.Log("[ItemTooltip] Initialized");
    }

    public void Show(ItemDataSO item, Vector3 slotPosition)
    {
        if (item == null)
        {
            Debug.LogWarning("[ItemTooltip] Item is null!");
            return;
        }

        Debug.Log($"[ItemTooltip] Showing tooltip for: {item.itemName}");

        // Название
        if (nameText != null)
        {
            nameText.text = item.itemName;
            //nameText.color = GetRarityColor(item.rarity);
        }

        // Описание
        if (descriptionText != null)
        {
            descriptionText.text = string.IsNullOrEmpty(item.description)
                ? "No description"
                : item.description;
        }

        // Статы
        if (statsText != null)
        {
            statsText.text = BuildStatsText(item);
        }

        // Полоска редкости
        if (rarityBar != null)
        {
            rarityBar.color = GetRarityColor(item.rarity);
        }

        // Позиция — над слотом
        Vector3 newPos = slotPosition + new Vector3(offset.x, offset.y, 0);
        tooltipPanel.transform.position = newPos;

        // Убедимся что тултип в пределах экрана
        ClampToScreen();

        tooltipPanel.SetActive(true);
    }

    public void Hide()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    private void ClampToScreen()
    {
        if (rectTransform == null || canvas == null) return;

        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        Vector3 pos = tooltipPanel.transform.position;

        // Правый край
        if (corners[2].x > screenWidth)
            pos.x -= corners[2].x - screenWidth + 10;

        // Левый край
        if (corners[0].x < 0)
            pos.x -= corners[0].x - 10;

        // Верхний край
        if (corners[1].y > screenHeight)
            pos.y -= corners[1].y - screenHeight + 10;

        // Нижний край
        if (corners[0].y < 0)
            pos.y -= corners[0].y - 10;

        tooltipPanel.transform.position = pos;
    }

    private string BuildStatsText(ItemDataSO item)
    {
        if (item.statModifiers == null || item.statModifiers.Count == 0)
            return "";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var mod in item.statModifiers)
        {
            string sign = mod.value >= 0 ? "+" : "";
            string valueStr;

            if (mod.modifierType == ModifierType.Percent)
            {
                valueStr = $"{sign}{mod.value * 100:F0}%";
            }
            else
            {
                valueStr = $"{sign}{mod.value:F1}";
            }

            sb.AppendLine($"{FormatStatName(mod.statType)}: {valueStr}");
        }

        return sb.ToString().TrimEnd();
    }

    private string FormatStatName(StatType type)
    {
        return type switch
        {
            StatType.MaxHealth => "Max HP",
            StatType.MoveSpeed => "Speed",
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

    private Color GetRarityColor(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => commonColor,
            Rarity.Uncommon => uncommonColor,
            Rarity.Rare => rareColor,
            Rarity.Legendary => legendaryColor,
            _ => commonColor
        };
    }
}