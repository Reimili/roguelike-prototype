// ==== MinimapUI.cs ====
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapUI : MonoBehaviour
{
    public static MinimapUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform mapContainer;
    [SerializeField] private GameObject roomIconPrefab;

    [Header("Settings")]
    [SerializeField] private float iconSize = 20f;
    [SerializeField] private float iconSpacing = 4f;
    [SerializeField] private float connectionThickness = 3f;

    [Header("Colors")]
    [SerializeField] private Color currentRoomColor = Color.white;
    [SerializeField] private Color visitedRoomColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color unvisitedRoomColor = new Color(0.25f, 0.25f, 0.25f, 0.6f);
    [SerializeField] private Color treasureColor = new Color(1f, 0.85f, 0.2f);
    [SerializeField] private Color bossColor = new Color(0.9f, 0.15f, 0.15f);
    [SerializeField] private Color startColor = new Color(0.3f, 0.8f, 0.3f);
    [SerializeField] private Color connectionColor = new Color(0.4f, 0.4f, 0.4f);

    // Данные
    private Dictionary<Vector2Int, MinimapRoomIcon> roomIcons = new();
    private Dictionary<Vector2Int, Room> allRooms;
    private HashSet<Vector2Int> visitedPositions = new();
    private HashSet<Vector2Int> revealedPositions = new();
    private Vector2Int currentPosition;

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Инициализация миникарты после генерации уровня
    /// </summary>
    public void Initialize(Dictionary<Vector2Int, Room> rooms)
    {
        allRooms = rooms;

        // Очищаем старые иконки
        foreach (Transform child in mapContainer)
        {
            Destroy(child.gameObject);
        }
        roomIcons.Clear();
        visitedPositions.Clear();
        revealedPositions.Clear();

        // Создаём иконки для всех комнат
        foreach (var kvp in allRooms)
        {
            CreateRoomIcon(kvp.Key, kvp.Value);
        }

        // Создаём соединения
        CreateConnections();

        // Посещаем стартовую комнату
        VisitRoom(Vector2Int.zero);
    }

    /// <summary>
    /// Вызывается при входе игрока в комнату
    /// </summary>
    public void VisitRoom(Vector2Int position)
    {
        currentPosition = position;

        // Отмечаем как посещённую
        visitedPositions.Add(position);
        revealedPositions.Add(position);

        // Раскрываем соседей
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var dir in dirs)
        {
            Vector2Int neighbor = position + dir;
            if (allRooms.ContainsKey(neighbor))
            {
                revealedPositions.Add(neighbor);
            }
        }

        // Обновляем все иконки
        UpdateAllIcons();
    }

    private void CreateRoomIcon(Vector2Int gridPos, Room room)
    {
        if (roomIconPrefab == null || mapContainer == null) return;

        GameObject iconObj = Instantiate(roomIconPrefab, mapContainer);
        iconObj.name = $"MapIcon_{gridPos.x}_{gridPos.y}";

        RectTransform rt = iconObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            float totalSize = iconSize + iconSpacing;
            rt.anchoredPosition = new Vector2(
                gridPos.x * totalSize,
                gridPos.y * totalSize
            );
            rt.sizeDelta = new Vector2(iconSize, iconSize);
        }

        MinimapRoomIcon icon = iconObj.GetComponent<MinimapRoomIcon>();
        if (icon == null)
            icon = iconObj.AddComponent<MinimapRoomIcon>();

        icon.Setup(room.roomType);

        // Начально скрыта
        iconObj.SetActive(false);

        roomIcons[gridPos] = icon;
    }

    private void CreateConnections()
    {
        HashSet<string> created = new();

        foreach (var kvp in allRooms)
        {
            Vector2Int pos = kvp.Key;
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.right };

            foreach (var dir in dirs)
            {
                Vector2Int neighbor = pos + dir;

                if (!allRooms.ContainsKey(neighbor)) continue;

                string key = $"{Mathf.Min(pos.x + pos.y * 1000, neighbor.x + neighbor.y * 1000)}_{Mathf.Max(pos.x + pos.y * 1000, neighbor.x + neighbor.y * 1000)}";
                if (created.Contains(key)) continue;
                created.Add(key);

                CreateConnectionLine(pos, neighbor);
            }
        }
    }

    private void CreateConnectionLine(Vector2Int from, Vector2Int to)
    {
        GameObject lineObj = new GameObject($"Connection_{from}_{to}");
        lineObj.transform.SetParent(mapContainer, false);

        Image lineImage = lineObj.AddComponent<Image>();
        lineImage.color = connectionColor;
        lineImage.raycastTarget = false;

        RectTransform rt = lineObj.GetComponent<RectTransform>();
        float totalSize = iconSize + iconSpacing;

        Vector2 fromPos = new Vector2(from.x * totalSize, from.y * totalSize);
        Vector2 toPos = new Vector2(to.x * totalSize, to.y * totalSize);
        Vector2 center = (fromPos + toPos) / 2f;

        rt.anchoredPosition = center;

        if (from.x != to.x)
        {
            // Горизонтальная линия
            rt.sizeDelta = new Vector2(iconSpacing, connectionThickness);
        }
        else
        {
            // Вертикальная линия
            rt.sizeDelta = new Vector2(connectionThickness, iconSpacing);
        }

        // Линии позади иконок
        lineObj.transform.SetAsFirstSibling();

        // Начально скрыта
        lineObj.SetActive(false);
        lineObj.AddComponent<MinimapConnection>().Setup(from, to);
    }

    private void UpdateAllIcons()
    {
        foreach (var kvp in roomIcons)
        {
            Vector2Int pos = kvp.Key;
            MinimapRoomIcon icon = kvp.Value;

            if (!revealedPositions.Contains(pos))
            {
                icon.gameObject.SetActive(false);
                continue;
            }

            icon.gameObject.SetActive(true);

            if (pos == currentPosition)
            {
                // Текущая комната — всегда белая
                icon.SetColor(currentRoomColor);
            }
            else if (visitedPositions.Contains(pos))
            {
                // Посещённая — цвет по типу
                icon.SetRoomTypeColor(
                    visitedRoomColor,
                    treasureColor,
                    bossColor,
                    startColor
                );
            }
            else
            {
                // Не посещённая но раскрытая — 
                // особые комнаты показываем цветом,
                // обычные — тёмным
                icon.SetRoomTypeColor(
                    unvisitedRoomColor,
                    treasureColor,
                    bossColor,
                    unvisitedRoomColor
                );
            }
        }

        // Обновляем соединения
        MinimapConnection[] connections = mapContainer.GetComponentsInChildren<MinimapConnection>(true);
        foreach (var conn in connections)
        {
            bool fromRevealed = revealedPositions.Contains(conn.From);
            bool toRevealed = revealedPositions.Contains(conn.To);
            conn.gameObject.SetActive(fromRevealed && toRevealed);
        }
    }

    public void Hide()
    {
        if (mapContainer != null)
            mapContainer.gameObject.SetActive(false);
    }

    public void Show()
    {
        if (mapContainer != null)
            mapContainer.gameObject.SetActive(true);
    }
}