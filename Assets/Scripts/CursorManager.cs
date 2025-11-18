using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D cursorTexture; // tu PNG
    public Vector2 hotSpot = Vector3.zero; // punto activo del cursor
    public CursorMode cursorMode = CursorMode.Auto;

    void Start()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
    }
}
