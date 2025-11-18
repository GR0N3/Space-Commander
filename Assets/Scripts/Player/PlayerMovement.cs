using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 180f;

    [Header("Mouse Tracking")]
    [SerializeField] private bool rotateTowardMouse = true;

    private Camera mainCamera;
    [SerializeField] private SpawnArea clampArea;
    [SerializeField] private bool clampToArea = true;
    [SerializeField] private float playerPadding = 0.5f;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        float verticalInput = Input.GetAxis("Vertical"); // W/S
        // Movimiento en la dirección forward del transform (ya está en el plano horizontal)
        Vector3 moveDirection = transform.forward * verticalInput;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        if (clampToArea && clampArea != null)
        {
            Vector3 pos = transform.position;
            if (clampArea.shape == SpawnArea.Shape.Circle)
            {
                Vector2 center = new Vector2(clampArea.transform.position.x, clampArea.transform.position.z);
                Vector2 p = new Vector2(pos.x, pos.z);
                Vector2 d = p - center;
                float allowed = Mathf.Max(0f, clampArea.radius - playerPadding);
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
                float hx = clampArea.size.x * 0.5f - playerPadding;
                float hz = clampArea.size.y * 0.5f - playerPadding;
                pos.x = Mathf.Clamp(pos.x, c.x - hx, c.x + hx);
                pos.z = Mathf.Clamp(pos.z, c.z - hz, c.z + hz);
            }
            transform.position = pos;
        }
    }

    private void HandleRotation()
    {
        if (rotateTowardMouse)
        {
            // Raycast desde la cámara hacia el plano donde está la nave
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, transform.position); // Plano horizontal en la posición de la nave
            
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 mouseWorldPos = ray.GetPoint(distance);
                
                // Calcular dirección hacia el mouse en el plano horizontal
                Vector3 direction = (mouseWorldPos - transform.position);
                direction.y = 0; // Mantener en el plano horizontal
                direction.Normalize();
                
                if (direction != Vector3.zero)
                {
                    // Calcular el ángulo en el plano horizontal (XZ)
                    float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                    
                    // Rotar solo en Y (eje vertical) para mantener la nave en el plano horizontal
                    Quaternion targetRotation = Quaternion.Euler(0f, angle, 0f);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation, 
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
                }
            }
        }
        else
        {
            float horizontalInput = Input.GetAxis("Horizontal"); // A/D
            // Rotar en el eje Y (horizontal)
            transform.Rotate(Vector3.up, horizontalInput * rotationSpeed * Time.deltaTime);
        }
    }
}
