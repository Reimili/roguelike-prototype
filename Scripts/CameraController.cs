// ==== CameraController.cs ====
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float transitionSpeed = 8f;
    [SerializeField] private float roomWidth = 16f;
    [SerializeField] private float roomHeight = 9f;

    private Vector3 targetPosition;
    private bool isTransitioning = false;
    private Room currentRoom;

    // Зум камеры
    private float targetOrthoSize;
    private float defaultOrthoSize;
    private bool isZooming = false;
    private float zoomSpeed = 3f;

    public bool IsTransitioning => isTransitioning;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        targetPosition = new Vector3(0, 0, transform.position.z);
        transform.position = targetPosition;

        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            defaultOrthoSize = roomHeight / 2f;
            cam.orthographicSize = defaultOrthoSize;
            targetOrthoSize = defaultOrthoSize;
        }
    }

    void LateUpdate()
    {
        // Плавное перемещение
        if (isTransitioning)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                transitionSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isTransitioning = false;
            }
        }

        // Плавный зум
        if (isZooming)
        {
            Camera cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographicSize = Mathf.Lerp(
                    cam.orthographicSize,
                    targetOrthoSize,
                    zoomSpeed * Time.deltaTime
                );

                if (Mathf.Abs(cam.orthographicSize - targetOrthoSize) < 0.01f)
                {
                    cam.orthographicSize = targetOrthoSize;
                    isZooming = false;
                }
            }
        }
    }

    public void MoveToRoom(Room room)
    {
        if (room == null || room == currentRoom) return;

        currentRoom = room;

        Vector3 roomCenter = new Vector3(
            room.gridPosition.x * roomWidth,
            room.gridPosition.y * roomHeight,
            transform.position.z
        );

        targetPosition = roomCenter;
        isTransitioning = true;
    }

    public void SnapToRoom(Room room)
    {
        if (room == null) return;

        currentRoom = room;

        Vector3 roomCenter = new Vector3(
            room.gridPosition.x * roomWidth,
            room.gridPosition.y * roomHeight,
            transform.position.z
        );

        targetPosition = roomCenter;
        transform.position = roomCenter;
        isTransitioning = false;
    }

    /// <summary>
    /// Подстроить камеру под размер арены босса
    /// </summary>
    public void AdjustToArenaSize(Vector2 arenaSize, float speed)
    {
        float targetSize = arenaSize.y / 2f;

        // Учитываем соотношение сторон экрана
        float screenAspect = (float)Screen.width / Screen.height;
        float arenaAspect = arenaSize.x / arenaSize.y;

        if (arenaAspect > screenAspect)
        {
            targetSize = (arenaSize.x / screenAspect) / 2f;
        }

        targetOrthoSize = targetSize;
        zoomSpeed = speed;
        isZooming = true;

        Debug.Log($"[Camera] Zooming to size {targetSize} for arena {arenaSize}");
    }

    /// <summary>
    /// Вернуть камеру к стандартному размеру
    /// </summary>
    public void ResetToDefaultSize(float speed = 3f)
    {
        targetOrthoSize = defaultOrthoSize;
        zoomSpeed = speed;
        isZooming = true;
    }
}