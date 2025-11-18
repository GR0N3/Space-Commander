using System.Collections.Generic;
using UnityEngine;

public class AreaEnemySpawner : MonoBehaviour, ISpawner
{
    public SpawnArea area;
    public GameObject enemyPrefab;
    public EnemyFactory factory;
    public bool useRandomEnemy = true;
    public string enemyId;
    public int initialCount = 3;
    public float minSeparation = 2f;
    public WaypointFactory waypointFactory;
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < initialCount; i++)
        {
            SpawnEnemy();
        }
    }

    public GameObject SpawnEnemy()
    {
        Vector3 pos = FindValidSpawnPoint();
        GameObject go = null;
        if (factory != null)
        {
            if (useRandomEnemy)
            {
                go = factory.CreateRandomEnemy(pos);
            }
            else if (!string.IsNullOrEmpty(enemyId))
            {
                go = factory.CreateEnemy(enemyId, pos);
            }
        }
        if (go == null && enemyPrefab != null)
        {
            go = Instantiate(enemyPrefab, pos, Quaternion.identity);
        }
        if (go == null) return null;
        var ec = go.GetComponent<EnemyController>();
        if (ec != null && waypointFactory != null)
        {
            ec.patrolPoints = waypointFactory.GetWaypoints();
            ec.restrictToPatrolArea = true;
        }
        var resp = go.GetComponent<RespawnOnDeath>();
        if (resp == null) resp = go.AddComponent<RespawnOnDeath>();
        resp.spawnerBehaviour = this;
        activeEnemies.Add(go);
        return go;
    }

    private Vector3 FindValidSpawnPoint()
    {
        for (int i = 0; i < 100; i++)
        {
            Vector3 p = area != null ? area.GetRandomPoint() : transform.position;
            Collider[] cols = Physics.OverlapSphere(p, minSeparation);
            bool ok = true;
            for (int c = 0; c < cols.Length; c++)
            {
                if (cols[c].CompareTag("Enemy"))
                {
                    ok = false;
                    break;
                }
            }
            if (ok) return p;
        }
        return area != null ? area.transform.position : transform.position;
    }
}
