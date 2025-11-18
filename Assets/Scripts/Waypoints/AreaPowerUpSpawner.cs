using UnityEngine;

public class AreaPowerUpSpawner : MonoBehaviour
{
    public SpawnArea area;
    public PowerUpPool pool;
    public GameObject prefab;
    public PowerUpType[] availableTypes;
    public int initialCount = 0;
    public float minSeparation = 2f;
    public Transform player;
    public bool spawnNearPlayer = true;
    public float nearRadius = 6f;

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        for (int i = 0; i < initialCount; i++)
        {
            SpawnPowerUp();
        }
    }

    public GameObject SpawnPowerUp()
    {
        Vector3 pos = FindValidSpawnPoint();
        GameObject go = null;
        if (pool != null)
        {
            go = pool.Get();
            go.transform.position = pos;
            go.transform.rotation = Quaternion.identity;
            go.SetActive(true);
        }
        else if (prefab != null)
        {
            go = Instantiate(prefab, pos, Quaternion.identity);
        }
        if (go == null) return null;
        var p = go.GetComponent<PowerUp>();
        if (p == null) p = go.AddComponent<PowerUp>();
        if (availableTypes != null && availableTypes.Length > 0)
        {
            p.powerUpType = availableTypes[Random.Range(0, availableTypes.Length)];
        }
        return go;
    }

    private Vector3 FindValidSpawnPoint()
    {
        for (int i = 0; i < 100; i++)
        {
            Vector3 p = GetCandidatePoint();
            if (area != null)
            {
                if (!IsInsideArea(p))
                {
                    p = area.GetRandomPoint();
                }
            }
            Collider[] cols = Physics.OverlapSphere(p, minSeparation);
            bool ok = true;
            for (int c = 0; c < cols.Length; c++)
            {
                if (cols[c].CompareTag("Enemy") || cols[c].CompareTag("Player"))
                {
                    ok = false;
                    break;
                }
            }
            if (ok) return p;
        }
        return area != null ? area.transform.position : transform.position;
    }

    private Vector3 GetCandidatePoint()
    {
        if (spawnNearPlayer && player != null)
        {
            Vector2 r = Random.insideUnitCircle * nearRadius;
            return new Vector3(player.position.x + r.x, player.position.y, player.position.z + r.y);
        }
        return area != null ? area.GetRandomPoint() : transform.position;
    }

    private bool IsInsideArea(Vector3 p)
    {
        if (area == null) return true;
        if (area.shape == SpawnArea.Shape.Circle)
        {
            Vector2 center = new Vector2(area.transform.position.x, area.transform.position.z);
            Vector2 pos = new Vector2(p.x, p.z);
            return Vector2.Distance(center, pos) <= area.radius;
        }
        Vector3 c = area.transform.position;
        float hx = area.size.x * 0.5f;
        float hz = area.size.y * 0.5f;
        return p.x >= c.x - hx && p.x <= c.x + hx && p.z >= c.z - hz && p.z <= c.z + hz;
    }
}
