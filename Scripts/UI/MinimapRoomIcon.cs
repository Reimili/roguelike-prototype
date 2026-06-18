// ==== MinimapRoomIcon.cs ====
using UnityEngine;
using UnityEngine.UI;

public class MinimapRoomIcon : MonoBehaviour
{
    private Image image;
    private RoomType roomType;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Setup(RoomType type)
    {
        roomType = type;

        if (image == null)
            image = GetComponent<Image>();
    }

    public void SetColor(Color color)
    {
        if (image != null)
            image.color = color;
    }

    public void SetRoomTypeColor(Color defaultColor, Color treasure, Color boss, Color start)
    {
        Color color = roomType switch
        {
            RoomType.Treasure => treasure,
            RoomType.Boss => boss,
            RoomType.Start => start,
            _ => defaultColor
        };

        SetColor(color);
    }
}