// ==== StatsPanelUI.cs ====
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class StatsPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform statsContainer;
    [SerializeField] private GameObject statRowPrefab;

    [Header("Settings")]
    [SerializeField] private bool startVisible = false;

    private Dictionary<StatType, TextMeshProUGUI> statTexts = new Dictionary<StatType, TextMeshProUGUI>();
    private bool isCreated = false;

    private readonly StatType[] displayedStats = {
        StatType.MaxHealth,
        StatType.MoveSpeed,
        StatType.Damage,
        StatType.FireRate,
        StatType.BulletSpeed,
        StatType.DashCooldown,
        StatType.Armor,
        StatType.CritChance,
        StatType.CritMultiplier,
        StatType.DodgeChance,
        StatType.HealthRegen
    };

    void Start()
    {
        Debug.Log("[StatsPanelUI] Start");

        if (statsContainer == null)
        {
            Debug.LogError("[StatsPanelUI] StatsContainer is not assigned!");
            return;
        }

        if (statRowPrefab == null)
        {
            Debug.LogError("[StatsPanelUI] StatRowPrefab is not assigned!");
            return;
        }

        CreateStatRows();

        // Начальное состояние
        gameObject.SetActive(startVisible);

        Debug.Log($"[StatsPanelUI] Started. Visible: {startVisible}");
    }

    public void Toggle()
    {
        bool newState = !gameObject.activeSelf;
        gameObject.SetActive(newState);

        Debug.Log($"[StatsPanelUI] Toggled. Now visible: {newState}");

        if (newState)
        {
            UpdateStats();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        UpdateStats();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void CreateStatRows()
    {
        if (isCreated) return;

        Debug.Log($"[StatsPanelUI] Creating {displayedStats.Length} stat rows");

        // Очищаем контейнер
        foreach (Transform child in statsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var statType in displayedStats)
        {
            GameObject row = Instantiate(statRowPrefab, statsContainer);
            row.name = $"StatRow_{statType}";

            StatRowUI rowUI = row.GetComponent<StatRowUI>();

            if (rowUI != null)
            {
                rowUI.Setup(statType);

                if (rowUI.ValueText != null)
                {
                    statTexts[statType] = rowUI.ValueText;
                }
            }
        }

        isCreated = true;
        Debug.Log($"[StatsPanelUI] Created {statTexts.Count} stat rows");
    }

    public void UpdateStats()
    {
        if (PlayerStats.Instance == null)
        {
            Debug.LogWarning("[StatsPanelUI] PlayerStats.Instance is null!");
            return;
        }

        foreach (var statType in displayedStats)
        {
            if (statTexts.TryGetValue(statType, out var text) && text != null)
            {
                float value = PlayerStats.Instance.GetStat(statType);
                text.text = FormatStatValue(statType, value);
            }
        }

        Debug.Log("[StatsPanelUI] Stats updated");
    }

    private string FormatStatValue(StatType type, float value)
    {
        return type switch
        {
            StatType.CritChance => $"{value * 100:F0}%",
            StatType.DodgeChance => $"{value * 100:F0}%",
            StatType.CritMultiplier => $"x{value:F1}",
            StatType.FireRate => $"{value:F2}s",
            StatType.DashCooldown => $"{value:F1}s",
            StatType.HealthRegen => $"{value:F1}/s",
            _ => $"{value:F1}"
        };
    }
}