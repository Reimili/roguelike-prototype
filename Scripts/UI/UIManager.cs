// ==== UIManager.cs ====
using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private HealthBarUI healthBar;
    [SerializeField] private InventoryBarUI inventoryBar;
    [SerializeField] private StatsPanelUI statsPanel;

    [Header("Stats Panel Settings")]
    [SerializeField] private KeyCode statsPanelHoldKey = KeyCode.Tab;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private bool isInitialized = false;
    private bool statsPanelVisible = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(InitializeWithRetry());
    }

    void Update()
    {
        // Показать пока удерживается Tab
        if (Input.GetKeyDown(statsPanelHoldKey))
        {
            // Нажали Tab — показываем
            if (statsPanel != null && !statsPanelVisible)
            {
                statsPanel.Show();
                statsPanelVisible = true;

                if (showDebugLogs)
                    Debug.Log("[UIManager] Stats panel shown (holding)");
            }
        }

        if (Input.GetKeyUp(statsPanelHoldKey))
        {
            // Отпустили Tab — скрываем
            if (statsPanel != null && statsPanelVisible)
            {
                statsPanel.Hide();
                statsPanelVisible = false;

                if (showDebugLogs)
                    Debug.Log("[UIManager] Stats panel hidden (released)");
            }
        }
    }

    IEnumerator InitializeWithRetry()
    {
        int attempts = 0;
        int maxAttempts = 10;

        while (!isInitialized && attempts < maxAttempts)
        {
            yield return null;
            attempts++;
            TryInitialize();
        }

        if (!isInitialized)
        {
            Debug.LogError("[UIManager] Failed to initialize after multiple attempts!");
        }
    }

    void TryInitialize()
    {
        bool allReady = true;

        // Health
        if (PlayerHealth.Instance != null && healthBar != null)
        {
            PlayerHealth.Instance.OnHealthChanged -= healthBar.UpdateHealth;
            PlayerHealth.Instance.OnHealthChanged += healthBar.UpdateHealth;
            healthBar.UpdateHealth(PlayerHealth.Instance.CurrentHealth, PlayerHealth.Instance.MaxHealth);

            if (showDebugLogs)
                Debug.Log("[UIManager] HealthBar connected");
        }
        else
        {
            allReady = false;
        }

        // Inventory
        if (Inventory.Instance != null && inventoryBar != null)
        {
            Inventory.Instance.OnItemAdded -= inventoryBar.AddItem;
            Inventory.Instance.OnItemAdded += inventoryBar.AddItem;
            Inventory.Instance.OnItemRemoved -= inventoryBar.RemoveItem;
            Inventory.Instance.OnItemRemoved += inventoryBar.RemoveItem;

            if (showDebugLogs)
                Debug.Log("[UIManager] InventoryBar connected");
        }
        else
        {
            allReady = false;
        }

        // Stats
        if (PlayerStats.Instance != null && statsPanel != null)
        {
            PlayerStats.Instance.OnStatsChanged -= statsPanel.UpdateStats;
            PlayerStats.Instance.OnStatsChanged += statsPanel.UpdateStats;

            if (showDebugLogs)
                Debug.Log("[UIManager] StatsPanel connected");
        }
        else
        {
            allReady = false;
        }

        isInitialized = allReady;

        if (isInitialized && showDebugLogs)
            Debug.Log("[UIManager] Fully initialized!");
    }

    void OnDestroy()
    {
        if (PlayerHealth.Instance != null && healthBar != null)
            PlayerHealth.Instance.OnHealthChanged -= healthBar.UpdateHealth;

        if (Inventory.Instance != null && inventoryBar != null)
        {
            Inventory.Instance.OnItemAdded -= inventoryBar.AddItem;
            Inventory.Instance.OnItemRemoved -= inventoryBar.RemoveItem;
        }

        if (PlayerStats.Instance != null && statsPanel != null)
            PlayerStats.Instance.OnStatsChanged -= statsPanel.UpdateStats;
    }
}