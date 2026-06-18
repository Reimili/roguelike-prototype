// ==== BossRoom.cs ====
using UnityEngine;
using System.Collections;

public class BossRoom : MonoBehaviour
{
    [Header("Arena")]
    [SerializeField] private GameObject arenaPrefab;
    [SerializeField] private Vector2 arenaSize = new Vector2(24f, 16f);

    [Header("Boss")]
    [SerializeField] private EnemyDataSO bossData;

    [Header("Transition")]
    [SerializeField] private float cameraZoomSpeed = 3f;

    [Header("Exit Portal")]
    [SerializeField] private GameObject exitPortalPrefab;

    private Room room;
    private bool isActivated = false;
    private GameObject arenaInstance;

    void Awake()
    {
        room = GetComponent<Room>();
    }

    public void SetBossData(GameObject arena, EnemyDataSO boss, GameObject portal)
    {
        arenaPrefab = arena;
        bossData = boss;
        exitPortalPrefab = portal;
    }

    public void StartBossSequence()
    {
        if (isActivated) return;
        isActivated = true;

        StartCoroutine(BossSequenceCoroutine());
    }

    private IEnumerator BossSequenceCoroutine()
    {
        Debug.Log("[BossRoom] Starting boss sequence...");

        // 1. Замораживаем игрока
        FreezePlayer(true);
        // Скрываем миникарту
        if (MinimapUI.Instance != null)
        {
            MinimapUI.Instance.Hide();
        }

        yield return new WaitForSeconds(0.5f);

        // 2. Центр арены
        Vector3 arenaCenter = transform.position;

        // 3. Скрываем ВСЮ карту КРОМЕ этой комнаты
        HideEntireMap();

        // 4. Скрываем визуал самой комнаты босса (но не отключаем объект!)
        HideBossRoomVisuals();

        // 5. Создаём арену
        SpawnArena(arenaCenter);

        // 6. Телепортируем игрока
        TeleportPlayer(arenaCenter);

        // 7. Подстраиваем камеру
        if (CameraController.Instance != null)
        {
            CameraController.Instance.AdjustToArenaSize(arenaSize, cameraZoomSpeed);
        }

        yield return new WaitForSeconds(1f);

        // 8. Спавним босса
        SpawnBoss(arenaCenter);

        // 9. Размораживаем игрока
        FreezePlayer(false);

        Debug.Log("[BossRoom] FIGHT!");
    }

    private void HideEntireMap()
    {
        MapGenerator mapGen = FindObjectOfType<MapGenerator>();
        if (mapGen == null) return;

        // Скрываем все комнаты КРОМЕ этой
        foreach (Transform child in mapGen.transform)
        {
            if (child.gameObject != gameObject)
            {
                child.gameObject.SetActive(false);
            }
        }

        Debug.Log("[BossRoom] Map hidden (except boss room)");
    }

    private void HideBossRoomVisuals()
    {
        // Скрываем спрайты комнаты босса, но НЕ отключаем объект
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = false;

        // Скрываем двери
        if (room.doorUp) room.doorUp.SetActive(false);
        if (room.doorDown) room.doorDown.SetActive(false);
        if (room.doorLeft) room.doorLeft.SetActive(false);
        if (room.doorRight) room.doorRight.SetActive(false);

        // Скрываем дочерние спрайты (стены, пол, декор)
        foreach (SpriteRenderer childSr in GetComponentsInChildren<SpriteRenderer>())
        {
            childSr.enabled = false;
        }

        // Отключаем дочерние коллайдеры (стены комнаты)
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            // Не отключаем RoomTrigger
            if (col.GetComponent<RoomTrigger>() == null)
            {
                col.enabled = false;
            }
        }

        Debug.Log("[BossRoom] Boss room visuals hidden");
    }

    private void SpawnArena(Vector3 center)
    {
        if (arenaPrefab == null)
        {
            Debug.LogError("[BossRoom] Arena prefab not assigned!");
            return;
        }

        arenaInstance = Instantiate(arenaPrefab, center, Quaternion.identity);
        arenaInstance.name = "BossArena";

        Debug.Log($"[BossRoom] Arena spawned at {center}");
    }

    private void SpawnBoss(Vector3 arenaCenter)
    {
        if (bossData == null || bossData.prefab == null)
        {
            Debug.LogError("[BossRoom] Boss data or prefab not assigned!");
            return;
        }

        Vector3 spawnPos = arenaCenter + new Vector3(0, 3f, 0);

        GameObject bossObj = Instantiate(
            bossData.prefab,
            spawnPos,
            Quaternion.identity,
            arenaInstance.transform
        );
        bossObj.name = bossData.enemyName;

        BaseEnemy bossEnemy = bossObj.GetComponent<BaseEnemy>();
        if (bossEnemy != null)
        {
            int level = room != null ? room.currentLevel : 1;
            bossEnemy.Initialize(bossData, level);
        }

        BossEnemy boss = bossObj.GetComponent<BossEnemy>();
        if (boss != null)
        {
            boss.OnBossDeath += OnBossDefeated;
        }

        Debug.Log($"[BossRoom] Boss '{bossData.enemyName}' spawned!");
    }

    private void TeleportPlayer(Vector3 arenaCenter)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        playerObj.transform.position = arenaCenter + new Vector3(0, -3f, 0);

        Debug.Log("[BossRoom] Player teleported to arena");
    }

    private void FreezePlayer(bool frozen)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
        if (playerRb != null && frozen)
        {
            playerRb.velocity = Vector2.zero;
        }

        PlayerMovement movement = playerObj.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.SetFrozen(frozen);
        }

        PlayerShooting shooting = playerObj.GetComponent<PlayerShooting>();
        if (shooting != null)
        {
            shooting.enabled = !frozen;
        }
    }

    private void OnBossDefeated()
    {
        Debug.Log("[BossRoom] Boss defeated!");

        SpawnBossReward();
        SpawnExitPortal();
    }

    private void SpawnExitPortal()
    {
        if (exitPortalPrefab == null)
        {
            Debug.LogWarning("[BossRoom] Exit portal prefab not assigned!");
            return;
        }

        Vector3 spawnPos = arenaInstance != null
            ? arenaInstance.transform.position + new Vector3(0, -2f, 0)
            : transform.position;

        Instantiate(exitPortalPrefab, spawnPos, Quaternion.identity);

        Debug.Log("[BossRoom] Exit portal spawned!");
    }

    private void SpawnBossReward()
    {
        if (ItemDatabase.Instance == null)
        {
            Debug.LogWarning("[BossRoom] ItemDatabase not found!");
            return;
        }

        // Получаем предмет из пула Boss
        ItemDataSO reward = ItemDatabase.Instance.GetRandomItem(ItemPool.Boss);

        if (reward == null)
        {
            Debug.LogWarning("[BossRoom] No items in Boss pool!");
            return;
        }

        // Находим центр арены
        Vector3 spawnPos = arenaInstance != null
            ? arenaInstance.transform.position
            : transform.position;

        // Ищем префаб WorldItem
        GameObject worldItemPrefab = GetWorldItemPrefab();

        if (worldItemPrefab == null)
        {
            Debug.LogError("[BossRoom] WorldItem prefab not found!");
            return;
        }

        // Спавним предмет
        GameObject itemObj = Instantiate(worldItemPrefab, spawnPos, Quaternion.identity);

        WorldItem worldItem = itemObj.GetComponent<WorldItem>();
        if (worldItem != null)
        {
            worldItem.Initialize(reward);
            Debug.Log($"[BossRoom] Reward spawned: {reward.itemName}");
        }
    }

    private GameObject GetWorldItemPrefab()
    {
        // Берём из комнаты если назначен
        if (room != null && room.worldItemPrefab != null)
            return room.worldItemPrefab;

        // Запасной вариант — ищем в ресурсах
        return Resources.Load<GameObject>("WorldItem");
    }
}