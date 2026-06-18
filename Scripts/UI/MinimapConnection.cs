// ==== MinimapConnection.cs ====
using UnityEngine;

public class MinimapConnection : MonoBehaviour
{
    public Vector2Int From { get; private set; }
    public Vector2Int To { get; private set; }

    public void Setup(Vector2Int from, Vector2Int to)
    {
        From = from;
        To = to;
    }
}