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
        float verticalInput = Input.GetAxis("Vertical");
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
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, transform.position);
            
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 mouseWorldPos = ray.GetPoint(distance);
                
                Vector3 direction = (mouseWorldPos - transform.position);
                direction.y = 0;
                direction.Normalize();
                
                if (direction != Vector3.zero)
                {
                    float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                    
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
            float horizontalInput = Input.GetAxis("Horizontal");
            transform.Rotate(Vector3.up, horizontalInput * rotationSpeed * Time.deltaTime);
        }
    }
}
