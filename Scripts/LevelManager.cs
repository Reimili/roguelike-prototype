// ==== LevelManager.cs ====
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int startLevel = 1;

    private int currentLevel;
    private List<ItemDataSO> savedItems = new List<ItemDataSO>();
    private bool isLoading = false;

    public int CurrentLevel => currentLevel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentLevel = startLevel;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadNextLevel()
    {
        if (isLoading) return;
        isLoading = true;

        SaveInventory();

        currentLevel++;

        Debug.Log($"[LevelManager] Loading level {currentLevel}...");

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResetRun()
    {
        if (isLoading) return;
        isLoading = true;

        currentLevel = startLevel;
        savedItems.Clear();

        Debug.Log("[LevelManager] Run reset");

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        isLoading = false;
    }

    public void RestoreInventory()
    {
        if (Inventory.Instance == null) return;

        foreach (var item in savedItems)
        {
            Inventory.Instance.TryAddItem(item);
        }

        Debug.Log($"[LevelManager] Restored {savedItems.Count} items");
    }

    private void SaveInventory()
    {
        savedItems.Clear();

        if (Inventory.Instance == null) return;

        foreach (var item in Inventory.Instance.Items)
        {
            savedItems.Add(item);
        }

        Debug.Log($"[LevelManager] Saved {savedItems.Count} items");
    }
}