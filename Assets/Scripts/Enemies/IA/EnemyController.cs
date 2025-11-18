using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{

    [Header("Visual")]
    public Renderer meshRenderer;
    public Material greenMaterial;
    public Material redMaterial;
    public Material yellowMaterial;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 5f;
    [Tooltip("Si está activado, el enemigo no podrá salir del área definida por los puntos de patrulla")]
    public bool restrictToPatrolArea = true;
    [Tooltip("Si está activado, el enemigo rotará aleatoriamente mientras patrulla")]
    public bool enableRandomRotation = true;
    [Tooltip("Velocidad de rotación aleatoria")]
    public float randomRotationSpeed = 2f;
    [Tooltip("Tiempo mínimo entre rotaciones aleatorias (segundos)")]
    public float minRotationInterval = 2f;
    [Tooltip("Tiempo máximo entre rotaciones aleatorias (segundos)")]
    public float maxRotationInterval = 5f;
    private Transform currentTarget;
    private float nextRandomRotationTime;
    private Quaternion targetRandomRotation;
    private bool isRotatingRandomly;

    public IEnemyState CurrentState { get; set; }

    [Header("Vision")]
    public float visionRange = 10f;
    [Tooltip("Ángulo de visión en grados (cono de visión)")]
    public float visionAngle = 360f;
    [Tooltip("Si está activado, muestra el área de visión en el editor")]
    public bool showVisionGizmos = true;
    [Tooltip("Color del área de visión en el editor")]
    public Color visionGizmoColor = new Color(1f, 0f, 0f, 0.3f);
    public Transform player;
    
    [Header("Alert")]
    [Tooltip("Velocidad de rotación hacia el jugador cuando está en alerta")]
    public float rotationSpeed = 5f;
    [Tooltip("Duración en segundos que el enemigo permanece en alerta antes de atacar")]
    public float alertDuration = 1f;
    
    [Header("Chase")]
    [Tooltip("Velocidad de persecución del jugador")]
    public float chaseSpeed = 7f;
    [Tooltip("Distancia mínima para detenerse cerca del jugador")]
    public float stopDistance = 2f;
    [Tooltip("Si está activado, el enemigo puede salir del área de patrulla para perseguir al jugador")]
    public bool canLeavePatrolAreaToChase = true;
    
    [Header("Collision Avoidance")]
    [Tooltip("Si está activado, los enemigos se separarán cuando colisionen")]
    public bool enableCollisionAvoidance = true;
    [Tooltip("Radio de detección de otros enemigos")]
    public float avoidanceRadius = 2f;
    [Tooltip("Fuerza de separación cuando detecta otro enemigo")]
    public float avoidanceForce = 5f;
    [Tooltip("Tag de los enemigos para detectar colisiones")]
    public string enemyTag = "Enemy";

    [Header("Shooting")]
    [Tooltip("Prefab de la bala (solo necesario si no usas BulletPool)")]
    public GameObject projectilePrefab;
    [Tooltip("Si está activado, usará el BulletPool. Si no, usará Instantiate")]
    public bool useBulletPool = true;
    public Transform firePoint;
    public float fireRate = 1f;
    private float fireCooldown;
    [Tooltip("Datos (BulletData) usados para inicializar las balas disparadas por este enemigo")]
    public BulletData enemyBulletData;

    void Start()
    {
        EnsurePatrolPoints();
        CurrentState = new PatrollingState();
        CurrentState.EnterState(this);
        ScheduleNextRandomRotation();
    }
    
    public void ScheduleNextRandomRotation()
    {
        if (enableRandomRotation)
        {
            nextRandomRotationTime = Time.time + Random.Range(minRotationInterval, maxRotationInterval);
            isRotatingRandomly = false;
        }
    }
    
    public bool ShouldDoRandomRotation()
    {
        return enableRandomRotation && Time.time >= nextRandomRotationTime && !isRotatingRandomly;
    }
    
    public void StartRandomRotation()
    {
        if (enableRandomRotation)
        {
            float randomAngle = Random.Range(0f, 360f);
            targetRandomRotation = Quaternion.Euler(0, randomAngle, 0);
            isRotatingRandomly = true;
        }
    }
    
    public bool UpdateRandomRotation(float deltaTime)
    {
        if (!isRotatingRandomly) return false;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRandomRotation,
            randomRotationSpeed * deltaTime
        );
        float angleDifference = Quaternion.Angle(transform.rotation, targetRandomRotation);
        if (angleDifference < 5f)
        {
            isRotatingRandomly = false;
            ScheduleNextRandomRotation();
            return false;
        }
        
        return true;
    }

    void Update()
    {
        fireCooldown -= Time.deltaTime;
        if (enableCollisionAvoidance)
        {
            ApplyCollisionAvoidance();
        }
        
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            CurrentState.UpdateState(this, Time.deltaTime);
        }
    }
    
    
    private void ApplyCollisionAvoidance()
    {
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, avoidanceRadius);
        Vector3 avoidanceDirection = Vector3.zero;
        int enemyCount = 0;
        foreach (Collider col in nearbyEnemies)
        {
            if (col.CompareTag(enemyTag) && col.gameObject != gameObject)
            {
                Vector3 directionAway = (transform.position - col.transform.position);
                float distance = directionAway.magnitude;
                if (distance > 0.1f)
                {
                    directionAway.y = 0;
                    directionAway.Normalize();
                    float forceMultiplier = 1f - (distance / avoidanceRadius);
                    avoidanceDirection += directionAway * forceMultiplier;
                    enemyCount++;
                }
            }
        }
        if (enemyCount > 0 && avoidanceDirection != Vector3.zero)
        {
            avoidanceDirection.Normalize();
            Vector3 avoidanceMovement = avoidanceDirection * avoidanceForce * Time.deltaTime;
            transform.position += avoidanceMovement;
        }
    }

    public void SwitchState(IEnemyState newState)
    {
        CurrentState.ExitState(this);
        CurrentState = newState;
        CurrentState.EnterState(this);
    }

    public void Patrol()
    {
        if (currentTarget == null && patrolPoints != null && patrolPoints.Length > 0)
        {
            currentTarget = patrolPoints[Random.Range(0, patrolPoints.Length)];
        }
    }
    
    public void PickRandomPatrolPoint()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            if (patrolPoints.Length > 1 && currentTarget != null)
            {
                Transform newTarget;
                do
                {
                    newTarget = patrolPoints[Random.Range(0, patrolPoints.Length)];
                } while (newTarget == currentTarget);
                
                currentTarget = newTarget;
            }
            else
            {
                currentTarget = patrolPoints[Random.Range(0, patrolPoints.Length)];
            }
        }
    }
    
    public Transform GetCurrentPatrolTarget()
    {
        return currentTarget;
    }
    
    
    public Bounds GetPatrolAreaBounds()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return new Bounds(transform.position, Vector3.zero);
        }
        System.Collections.Generic.List<Transform> valid = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            var p = patrolPoints[i];
            if (p) valid.Add(p);
        }
        if (valid.Count == 0)
        {
            return new Bounds(transform.position, Vector3.zero);
        }
        Vector3 min = valid[0].position;
        Vector3 max = valid[0].position;
        for (int i = 1; i < valid.Count; i++)
        {
            var point = valid[i];
            min = Vector3.Min(min, point.position);
            max = Vector3.Max(max, point.position);
        }
        Vector3 center = (min + max) / 2f;
        Vector3 size = max - min;
        return new Bounds(center, size);
    }
    
    
    public bool IsPositionInPatrolArea(Vector3 position)
    {
        if (!restrictToPatrolArea || patrolPoints == null || patrolPoints.Length < 3)
        {
            return true;
        }
        Bounds bounds = GetPatrolAreaBounds();
        bounds.Expand(0.1f);
        return position.x >= bounds.min.x && position.x <= bounds.max.x &&
               position.z >= bounds.min.z && position.z <= bounds.max.z;
    }
    
    
    public Vector3 ClampPositionToPatrolArea(Vector3 position)
    {
        if (!restrictToPatrolArea || patrolPoints == null || patrolPoints.Length < 3)
        {
            return position;
        }
        Bounds bounds = GetPatrolAreaBounds();
        bounds.Expand(0.1f);
        float clampedX = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
        float clampedZ = Mathf.Clamp(position.z, bounds.min.z, bounds.max.z);
        return new Vector3(clampedX, position.y, clampedZ);
    }

    public void SetMaterial(Material mat)
    {
        if (meshRenderer != null && mat != null)
        {
            meshRenderer.material = mat;
        }
    }

    public bool IsPlayerInSight()
    {
        return Vector3.Distance(transform.position, player.position) <= visionRange;
    }

    private void EnsurePatrolPoints()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            var wf = Object.FindFirstObjectByType<WaypointFactory>();
            if (wf != null)
            {
                patrolPoints = wf.GetWaypoints();
                restrictToPatrolArea = true;
            }
            else
            {
                restrictToPatrolArea = false;
            }
        }
    }

    public void Shoot()
    {
        if (fireCooldown <= 0f)
        {
            if (enemyBulletData == null)
            {
                return;
            }
            if (firePoint == null)
            {
                return;
            }
            
            SetMaterial(yellowMaterial);

            GameObject spawnedBullet = null;
            if (useBulletPool && BulletPool.Instance != null)
            {
                spawnedBullet = BulletPool.Instance.GetBullet(firePoint.position, firePoint.rotation);
            }
            else if (projectilePrefab != null)
            {
                spawnedBullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            }
            else
            {
                return;
            }

            if (spawnedBullet != null)
            {
                Bullet b = spawnedBullet.GetComponent<Bullet>();
                if (b != null)
                {
                    b.Initialize(enemyBulletData);
                }
                else { }
            }

            if (enemyBulletData != null && enemyBulletData.shootSound != null && firePoint != null)
            {
                AudioSource.PlayClipAtPoint(enemyBulletData.shootSound, firePoint.position, enemyBulletData.shootVolume);
            }

            fireCooldown = fireRate;

            StartCoroutine(ResetToRed());
        }
    }

    private IEnumerator ResetToRed()
    {
        yield return new WaitForSeconds(0.1f);
        SetMaterial(redMaterial);
    }
    
    
    void OnDrawGizmos()
    {
        if (!showVisionGizmos) return;
        if (visionAngle >= 360f)
        {
            Gizmos.color = visionGizmoColor;
            DrawCircle(transform.position, visionRange, 64);
            Gizmos.color = new Color(visionGizmoColor.r, visionGizmoColor.g, visionGizmoColor.b, 0.1f);
            Gizmos.DrawWireSphere(transform.position, visionRange);
        }
        else
        {
            Gizmos.color = visionGizmoColor;
            DrawVisionCone(transform.position, transform.forward, visionRange, visionAngle, 64);
        }
        if (player != null)
        {
            if (IsPlayerInSight())
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, player.position);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(player.position, 0.5f);
            }
            else
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(transform.position, player.position);
            }
        }
    }
    
    
    void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
    
    void DrawVisionCone(Vector3 center, Vector3 forward, float range, float angle, int segments)
    {
        forward.y = 0;
        forward.Normalize();
        float forwardAngle = Mathf.Atan2(forward.z, forward.x) * Mathf.Rad2Deg;
        float halfAngle = angle * 0.5f;
        float startAngle = forwardAngle - halfAngle;
        float endAngle = forwardAngle + halfAngle;
        Vector3 leftDirection = Quaternion.Euler(0, startAngle, 0) * Vector3.right;
        Vector3 rightDirection = Quaternion.Euler(0, endAngle, 0) * Vector3.right;
        Gizmos.DrawLine(center, center + leftDirection * range);
        Gizmos.DrawLine(center, center + rightDirection * range);
        float angleStep = angle / segments;
        Vector3 prevPoint = center + leftDirection * range;
        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * Vector3.right;
            Vector3 newPoint = center + direction * range;
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}
