// ==== MapGenerator.cs ====
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject roomPrefab;
    public GameObject worldItemPrefab;

    [Header("Room Settings")]
    public int minRooms = 10;
    public int maxRooms = 18;
    public Vector2 roomSize = new Vector2(16f, 9f);

    [Header("Level")]
    public int currentLevel = 1;

    [Header("Boss Settings")]
    public GameObject arenaPrefab;
    public EnemyDataSO bossData;
    public GameObject exitPortalPrefab;

    private Dictionary<Vector2Int, Room> rooms = new();

    private static readonly Vector2Int[] Directions = {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    void Start()
    {
        if (LevelManager.Instance != null)
        {
            currentLevel = LevelManager.Instance.CurrentLevel;
        }

        if (ItemDatabase.Instance != null)
            ItemDatabase.Instance.ResetForNewRun();

        GenerateMap();
        AssignSpecialRooms();
        UpdateAllDoors();

        if (CameraController.Instance != null && rooms.ContainsKey(Vector2Int.zero))
        {
            CameraController.Instance.SnapToRoom(rooms[Vector2Int.zero]);
        }

        if (MinimapUI.Instance != null)
        {
            MinimapUI.Instance.Initialize(rooms);
        }

        // Восстанавливаем предметы с задержкой
        if (LevelManager.Instance != null)
        {
            StartCoroutine(RestoreItemsDelayed());
        }
    }

    private System.Collections.IEnumerator RestoreItemsDelayed()
    {
        // Ждём пока UIManager и Inventory полностью инициализируются
        yield return null;
        yield return null;

        LevelManager.Instance.RestoreInventory();
    }

    void GenerateMap()
    {
        Vector2Int start = Vector2Int.zero;
        rooms[start] = CreateRoom(start, RoomType.Start);

        var shuffledDirs = Directions.OrderBy(_ => Random.value).ToArray();
        int neighborCount = Random.value < 0.8f ? 3 : 4;

        Queue<Vector2Int> queue = new();

        for (int i = 0; i < neighborCount; i++)
        {
            Vector2Int pos = start + shuffledDirs[i];
            rooms[pos] = CreateRoom(pos, RoomType.Normal);
            queue.Enqueue(pos);
        }

        int created = 1 + neighborCount;
        int target = Random.Range(minRooms, maxRooms + 1);

        while (queue.Count > 0 && created < target)
        {
            Vector2Int current = queue.Dequeue();

            foreach (var dir in Directions.OrderBy(_ => Random.value))
            {
                Vector2Int next = current + dir;

                if (rooms.ContainsKey(next) || Random.value > 0.5f)
                    continue;

                rooms[next] = CreateRoom(next, RoomType.Normal);
                queue.Enqueue(next);
                created++;

                if (created >= target)
                    break;
            }
        }

        Debug.Log($"[MapGenerator] Generated {rooms.Count} rooms (Level {currentLevel})");
    }

    Room CreateRoom(Vector2Int gridPos, RoomType type)
    {
        Vector3 worldPos = new Vector3(
            gridPos.x * roomSize.x,
            gridPos.y * roomSize.y,
            0f
        );

        var obj = Instantiate(roomPrefab, worldPos, Quaternion.identity, transform);
        obj.name = $"Room_{gridPos.x}_{gridPos.y}";

        Room room = obj.GetComponent<Room>();
        room.gridPosition = gridPos;
        room.roomType = type;
        room.currentLevel = currentLevel;
        room.worldItemPrefab = worldItemPrefab;

        RoomTrigger trigger = obj.GetComponentInChildren<RoomTrigger>();
        if (trigger != null)
        {
            trigger.room = room;
        }

        return room;
    }

    void AssignSpecialRooms()
    {
        var candidates = rooms.Values
            .Where(r => r.roomType == RoomType.Normal)
            .ToList();

        if (candidates.Count < 2)
        {
            Debug.LogWarning("[MapGenerator] Not enough rooms!");
            return;
        }

        // Сокровищница
        Room treasure = candidates[Random.Range(0, candidates.Count)];
        treasure.roomType = RoomType.Treasure;
        treasure.SpawnTreasure();
        treasure.UpdateVisual();
        candidates.Remove(treasure);

        // Босс
        Room boss = candidates
            .OrderByDescending(r => Vector2Int.Distance(r.gridPosition, Vector2Int.zero))
            .First();
        boss.roomType = RoomType.Boss;
        boss.UpdateVisual();

        // Добавляем BossRoom и передаём данные
        BossRoom bossRoom = boss.gameObject.AddComponent<BossRoom>();
        bossRoom.SetBossData(arenaPrefab, bossData, exitPortalPrefab);

        Debug.Log($"[MapGenerator] Treasure: {treasure.gridPosition}, Boss: {boss.gridPosition}");
    }

    void UpdateAllDoors()
    {
        foreach (var room in rooms.Values)
            room.UpdateDoors(rooms);
    }

    public Dictionary<Vector2Int, Room> GetRooms() => rooms;
}