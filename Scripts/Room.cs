// ==== Room.cs ====
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomType { Start, Normal, Boss, Treasure }

public class Room : MonoBehaviour
{
    [Header("Room Info")]
    public Vector2Int gridPosition;
    public RoomType roomType;
    public bool isActivated = false;
    public int currentLevel = 1;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Sprite normalSprite, treasureSprite, bossSprite;

    [Header("Doors")]
    public GameObject doorUp, doorDown, doorLeft, doorRight;

    [Header("Enemy Spawn")]
    public GameObject spawnMarkerPrefab;
    public float markerDelay = 1.5f;

    [Header("Treasure")]
    public GameObject worldItemPrefab;

    // Приватные поля
    private readonly List<GameObject> enemies = new();
    private readonly List<Vector3> spawnPositions = new();
    private readonly List<GameObject> markers = new();
    private MapGenerator mapGenerator;

    void Awake()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
    }

    // ==================== ДВЕРИ ====================

    public void SetDoor(Vector2Int direction, bool isClosed)
    {
        GameObject door = GetDoor(direction);
        if (door != null) door.SetActive(isClosed);
    }

    private GameObject GetDoor(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return doorUp;
        if (direction == Vector2Int.down) return doorDown;
        if (direction == Vector2Int.left) return doorLeft;
        if (direction == Vector2Int.right) return doorRight;
        return null;
    }

    public void UpdateDoors(Dictionary<Vector2Int, Room> allRooms)
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var dir in dirs)
        {
            bool hasNeighbor = allRooms.ContainsKey(gridPosition + dir);
            SetDoor(dir, !hasNeighbor);
        }
    }

    private void CloseAllDoors()
    {
        if (doorUp) doorUp.SetActive(true);
        if (doorDown) doorDown.SetActive(true);
        if (doorLeft) doorLeft.SetActive(true);
        if (doorRight) doorRight.SetActive(true);
    }

    private void OpenConnectedDoors()
    {
        if (mapGenerator != null)
            UpdateDoors(mapGenerator.GetRooms());
    }

    // ==================== ВРАГИ ====================

    public void SpawnEnemies()
    {
        if (roomType == RoomType.Treasure || roomType == RoomType.Start)
            return;

        enemies.Clear();
        spawnPositions.Clear();

        if (EnemyDatabase.Instance == null)
        {
            Debug.LogError("[Room] EnemyDatabase not found!");
            return;
        }

        List<EnemyDataSO> enemyList = EnemyDatabase.Instance.GenerateRoomEnemies(currentLevel, roomType);

        if (enemyList.Count == 0)
        {
            Debug.LogWarning($"[Room] No enemies for level {currentLevel}");
            return;
        }

        for (int i = 0; i < enemyList.Count; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-4f, 4f),
                Random.Range(-2f, 2f),
                0f
            );
            spawnPositions.Add(transform.position + offset);

            if (spawnMarkerPrefab != null)
            {
                var marker = Instantiate(spawnMarkerPrefab, spawnPositions[i], Quaternion.identity, transform);
                markers.Add(marker);
            }
        }

        CloseAllDoors();
        StartCoroutine(SpawnEnemiesFromList(enemyList));
    }

    private IEnumerator SpawnEnemiesFromList(List<EnemyDataSO> enemyList)
    {
        yield return new WaitForSeconds(markerDelay);

        foreach (var marker in markers)
            Destroy(marker);
        markers.Clear();

        for (int i = 0; i < enemyList.Count; i++)
        {
            EnemyDataSO data = enemyList[i];

            if (data.prefab == null)
            {
                Debug.LogError($"[Room] Enemy '{data.enemyName}' has no prefab!");
                continue;
            }

            Vector3 pos = i < spawnPositions.Count
                ? spawnPositions[i]
                : transform.position;

            // Создаём врага
            GameObject enemyObj = Instantiate(data.prefab, pos, Quaternion.identity, transform);
            enemyObj.name = $"{data.enemyName}_{i}";

            // ← ВАЖНО: передаём данные из SO!
            BaseEnemy enemyScript = enemyObj.GetComponent<BaseEnemy>();
            if (enemyScript != null)
            {
                enemyScript.Initialize(data, currentLevel);
            }
            else
            {
                Debug.LogError($"[Room] {data.enemyName} prefab has no BaseEnemy component!");
            }

            enemies.Add(enemyObj);
        }

        Debug.Log($"[Room] Spawned {enemies.Count} enemies");
    }

    // Старый метод — для совместимости
    public void SpawnEnemies(GameObject enemyPrefab)
    {
        SpawnEnemies();
    }

    public void EnemyDefeated(GameObject enemy)
    {
        if (enemies.Contains(enemy))
            enemies.Remove(enemy);

        Debug.Log($"[Room] Enemy defeated. Remaining: {enemies.Count}");

        if (enemies.Count == 0)
        {
            Debug.Log($"[Room] {gridPosition} cleared!");
            OpenConnectedDoors();
        }
    }

    public bool IsCleared() => isActivated && enemies.Count == 0;

    // ==================== СОКРОВИЩНИЦА ====================

    public void SpawnTreasure()
    {
        if (roomType != RoomType.Treasure || worldItemPrefab == null) return;

        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("[Room] ItemDatabase not found!");
            return;
        }

        ItemDataSO randomItem = ItemDatabase.Instance.GetRandomItem(ItemPool.Treasure);
        if (randomItem == null) return;

        var itemObj = Instantiate(worldItemPrefab, transform.position, Quaternion.identity, transform);
        itemObj.GetComponent<WorldItem>()?.Initialize(randomItem);
        Debug.Log($"[Room] Treasure: {randomItem.itemName}");
    }

    public void RegisterEnemy(GameObject enemy)
    {
        if (!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
            Debug.Log($"[Room] Enemy registered. Total: {enemies.Count}");
        }
    }

    // ==================== ВИЗУАЛ ====================

    public void UpdateVisual()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.sprite = roomType switch
        {
            RoomType.Treasure => treasureSprite,
            RoomType.Boss => bossSprite,
            _ => normalSprite
        };
    }
}