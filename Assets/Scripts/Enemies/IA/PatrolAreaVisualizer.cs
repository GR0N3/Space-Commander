using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class PatrolAreaVisualizer : MonoBehaviour
{
    [Header("Visualización")]
    public bool showGizmos = true;
    
    public Color areaColor = Color.green;
    
    public Color pointColor = Color.yellow;
    
    private EnemyController enemyController;
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        if (enemyController == null)
        {
            enemyController = GetComponent<EnemyController>();
        }
        
        if (enemyController == null || enemyController.patrolPoints == null || enemyController.patrolPoints.Length < 3)
        {
            return;
        }
        
        Transform[] points = enemyController.patrolPoints;
        System.Collections.Generic.List<Transform> valid = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < points.Length; i++)
        {
            var p = points[i];
            if (p) valid.Add(p);
        }
        if (valid.Count < 3) return;
        
        Gizmos.color = areaColor;
        for (int i = 0; i < valid.Count; i++)
        {
            int next = (i + 1) % valid.Count;
            Gizmos.DrawLine(valid[i].position, valid[next].position);
        }
        Gizmos.color = pointColor;
        foreach (Transform point in valid)
        {
            Gizmos.DrawSphere(point.position, 0.3f);
        }
        if (enemyController.restrictToPatrolArea)
        {
            Bounds bounds = enemyController.GetPatrolAreaBounds();
            Gizmos.color = new Color(areaColor.r, areaColor.g, areaColor.b, 0.1f);
            Vector3 center = bounds.center;
            Vector3 size = bounds.size;
            size.y = 0.1f;
            center.y = bounds.center.y;
            Gizmos.DrawCube(center, size);
            Gizmos.color = areaColor;
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Gizmos.DrawLine(new Vector3(min.x, center.y, min.z), new Vector3(max.x, center.y, min.z));
            Gizmos.DrawLine(new Vector3(max.x, center.y, min.z), new Vector3(max.x, center.y, max.z));
            Gizmos.DrawLine(new Vector3(max.x, center.y, max.z), new Vector3(min.x, center.y, max.z));
            Gizmos.DrawLine(new Vector3(min.x, center.y, max.z), new Vector3(min.x, center.y, min.z));
        }
    }
}

