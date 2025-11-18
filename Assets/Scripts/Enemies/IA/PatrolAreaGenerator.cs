using UnityEngine;

public class PatrolAreaGenerator : MonoBehaviour
{
    [Header("Área de Patrulla")]
    public Vector3 areaCenter = Vector3.zero;
    
    public float areaWidth = 10f;
    
    public float areaLength = 10f;
    
    public float areaHeight = 0f;
    
    [Header("Configuración")]
    public bool generateOnStart = false;
    
    public bool showGizmos = true;
    
    private Transform[] generatedPoints = new Transform[4];
    
    void Start()
    {
        if (generateOnStart)
        {
            GeneratePatrolPoints();
        }
    }
    
    public void GeneratePatrolPoints()
    {
        ClearPatrolPoints();
        float halfWidth = areaWidth / 2f;
        float halfLength = areaLength / 2f;
        
        Vector3[] corners = new Vector3[4]
        {
            new Vector3(areaCenter.x - halfWidth, areaHeight, areaCenter.z - halfLength),
            new Vector3(areaCenter.x + halfWidth, areaHeight, areaCenter.z - halfLength),
            new Vector3(areaCenter.x + halfWidth, areaHeight, areaCenter.z + halfLength),
            new Vector3(areaCenter.x - halfWidth, areaHeight, areaCenter.z + halfLength)
        };
        for (int i = 0; i < 4; i++)
        {
            GameObject point = new GameObject($"PatrolPoint_{i + 1}");
            point.transform.position = corners[i];
            point.transform.SetParent(this.transform);
            generatedPoints[i] = point.transform;
        }
        EnemyController enemyController = GetComponent<EnemyController>();
        if (enemyController != null && (enemyController.patrolPoints == null || enemyController.patrolPoints.Length == 0))
        {
            enemyController.patrolPoints = generatedPoints;
        }
    }
    
    public Transform[] GetPatrolPoints()
    {
        return generatedPoints;
    }
    
    public void ClearPatrolPoints()
    {
        foreach (Transform point in generatedPoints)
        {
            if (point != null)
            {
                DestroyImmediate(point.gameObject);
            }
        }
        generatedPoints = new Transform[4];
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        float halfWidth = areaWidth / 2f;
        float halfLength = areaLength / 2f;
        
        Vector3[] corners = new Vector3[4]
        {
            new Vector3(areaCenter.x - halfWidth, areaHeight, areaCenter.z - halfLength),
            new Vector3(areaCenter.x + halfWidth, areaHeight, areaCenter.z - halfLength),
            new Vector3(areaCenter.x + halfWidth, areaHeight, areaCenter.z + halfLength),
            new Vector3(areaCenter.x - halfWidth, areaHeight, areaCenter.z + halfLength)
        };
        
        Gizmos.color = Color.green;
        for (int i = 0; i < 4; i++)
        {
            int next = (i + 1) % 4;
            Gizmos.DrawLine(corners[i], corners[next]);
        }
        Gizmos.color = Color.yellow;
        foreach (Vector3 corner in corners)
        {
            Gizmos.DrawSphere(corner, 0.3f);
        }
        if (generatedPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform point in generatedPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, 0.5f);
                }
            }
        }
    }
}

