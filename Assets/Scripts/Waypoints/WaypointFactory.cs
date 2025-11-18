using System.Collections.Generic;
using UnityEngine;

public class WaypointFactory : MonoBehaviour
{
    public SpawnArea area;
    public WaypointPool pool;
    public int waypointCount = 4;
    public float minDistance = 2f;
    private List<GameObject> active = new List<GameObject>();

    public Transform[] GetWaypoints()
    {
        if (active.Count == waypointCount) return active.ConvertAll(x => x.transform).ToArray();
        Clear();
        Generate();
        return active.ConvertAll(x => x.transform).ToArray();
    }

    public void Clear()
    {
        foreach (var go in active) pool.Return(go);
        active.Clear();
    }

    private void Generate()
    {
        int created = 0;
        int attempts = 0;
        while (created < waypointCount && attempts < 1000)
        {
            attempts++;
            Vector3 p = FindValidPoint();
            if (p == Vector3.positiveInfinity) break;
            GameObject wp = pool.Get();
            wp.transform.position = p;
            wp.transform.rotation = Quaternion.identity;
            wp.SetActive(true);
            active.Add(wp);
            created++;
        }
    }

    private Vector3 FindValidPoint()
    {
        for (int i = 0; i < 50; i++)
        {
            Vector3 p = area != null ? area.GetRandomPoint() : Vector3.zero;
            bool ok = true;
            for (int j = 0; j < active.Count; j++)
            {
                if (Vector3.Distance(p, active[j].transform.position) < minDistance)
                {
                    ok = false;
                    break;
                }
            }
            if (ok) return p;
        }
        return Vector3.positiveInfinity;
    }
}