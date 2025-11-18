using UnityEngine;

public class StaticCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Objetivo a seguir (el jugador)")]
    [SerializeField] private Transform target;
    
    [Header("Camera Offset")]
    [Tooltip("Offset relativo al jugador (posición fija desde donde seguir)")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 10f, 0f);
    
    [Tooltip("Si está activado, la cámara mantendrá su rotación inicial")]
    [SerializeField] private bool keepInitialRotation = true;
    
    [Header("Smoothing")]
    [Tooltip("Velocidad de seguimiento (0 = instantáneo, mayor = más suave)")]
    [SerializeField] private float followSpeed = 10f;
    
    private Quaternion initialRotation;
    private Vector3 targetPosition;
    [SerializeField] private SpawnArea clampArea;
    [SerializeField] private bool clampToArea = true;
    private Camera cam;
    [SerializeField] private float clampPadding = 0f;
    
    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
        initialRotation = transform.rotation;
        if (target != null)
        {
            targetPosition = target.position + offset;
            transform.position = targetPosition;
        }
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        targetPosition = target.position + offset;
        
        if (followSpeed > 0)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPosition;
        }

        if (clampToArea && clampArea != null && cam != null)
        {
            Vector3 pos = transform.position;
            float halfZ, halfX;
            if (cam.orthographic)
            {
                halfZ = cam.orthographicSize;
                halfX = halfZ * cam.aspect;
            }
            else
            {
                float height = Mathf.Abs(pos.y - clampArea.transform.position.y);
                halfZ = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * height;
                halfX = halfZ * cam.aspect;
            }
            halfX += clampPadding; halfZ += clampPadding;
            if (clampArea.shape == SpawnArea.Shape.Circle)
            {
                Vector2 center = new Vector2(clampArea.transform.position.x, clampArea.transform.position.z);
                Vector2 p = new Vector2(pos.x, pos.z);
                float corner = Mathf.Sqrt(halfX * halfX + halfZ * halfZ);
                float allowed = Mathf.Max(0f, clampArea.radius - corner);
                Vector2 d = p - center;
                float m = d.magnitude;
                if (m > allowed)
                {
                    d = d.normalized * allowed;
                    p = center + d;
                }
                pos.x = p.x; pos.z = p.y;
            }
            else
            {
                Vector3 c = clampArea.transform.position;
                float hx = clampArea.size.x * 0.5f;
                float hz = clampArea.size.y * 0.5f;
                pos.x = Mathf.Clamp(pos.x, c.x - hx + halfX, c.x + hx - halfX);
                pos.z = Mathf.Clamp(pos.z, c.z - hz + halfZ, c.z + hz - halfZ);
            }
            transform.position = pos;
        }
        
        if (keepInitialRotation)
        {
            transform.rotation = initialRotation;
        }
    }
}

