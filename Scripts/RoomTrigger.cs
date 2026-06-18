// ==== RoomTrigger.cs ====
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    [HideInInspector] public Room room;

    private bool playerInside = false;
    private float timer = 0f;
    private const float activationDelay = 0.7f;

    void Start()
    {
        if (room == null)
            room = GetComponentInParent<Room>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Камера
        if (CameraController.Instance != null)
        {
            CameraController.Instance.MoveToRoom(room);
        }

        // Миникарта
        if (MinimapUI.Instance != null)
        {
            MinimapUI.Instance.VisitRoom(room.gridPosition);
        }

        // Активация комнаты
        if (!room.isActivated)
        {
            playerInside = true;
            timer = 0f;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            timer = 0f;
        }
    }

    void Update()
    {
        if (room == null || room.isActivated || !playerInside) return;

        timer += Time.deltaTime;

        if (timer >= activationDelay)
        {
            room.isActivated = true;

            if (room.roomType == RoomType.Boss)
            {
                // Запускаем босс-сиквенс
                BossRoom bossRoom = room.GetComponent<BossRoom>();
                if (bossRoom != null)
                {
                    bossRoom.StartBossSequence();
                }
            }
            else
            {
                room.SpawnEnemies();
            }
        }
    }
}