using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Header("Cursor Settings")]
    [Tooltip("Textura del cursor (alta visibilidad)")]
    public Texture2D cursorTexture;

    [Tooltip("Punto de anclaje/clic del cursor. Si es (0,0), se centrará automáticamente")]
    public Vector2 hotSpot = Vector2.zero;

    public CursorMode cursorMode = CursorMode.Auto;

    [Header("Seguimiento de Cámara")]
    [Tooltip("Si está activo, este GameObject seguirá la posición y rotación de la cámara")]
    public bool followCamera = true;

    [Tooltip("Cámara objetivo. Si es null, buscará Camera.main automáticamente")]
    public Camera targetCamera;

    [Tooltip("Offset relativo a la cámara")]
    public Vector3 cameraOffset = Vector3.zero;

    void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void Start()
    {
        ApplyCursor();
    }

    void OnEnable()
    {
        ApplyCursor();
    }

    void LateUpdate()
    {
        if (followCamera)
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera != null)
            {
                transform.position = targetCamera.transform.position + cameraOffset;
                transform.rotation = targetCamera.transform.rotation;
            }
        }
    }

    public void ApplyCursor()
    {
        if (cursorTexture != null)
        {
            Vector2 actualHotspot = hotSpot;
            if (actualHotspot == Vector2.zero)
            {
                actualHotspot = new Vector2(cursorTexture.width * 0.5f, cursorTexture.height * 0.5f);
            }
            Cursor.SetCursor(cursorTexture, actualHotspot, cursorMode);
        }
    }
}
