using System.Collections.Generic;
using UnityEngine;

public class WaypointPool : MonoBehaviour
{
    public GameObject waypointPrefab;
    public int poolSize = 20;
    private List<GameObject> pool = new List<GameObject>();

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var go = Instantiate(waypointPrefab);
            go.SetActive(false);
            pool.Add(go);
        }
    }

    public GameObject Get()
    {
        foreach (var go in pool)
        {
            if (!go.activeInHierarchy) return go;
        }
        var n = Instantiate(waypointPrefab);
        n.SetActive(false);
        pool.Add(n);
        return n;
    }

    public void Return(GameObject go)
    {
        if (go == null) return;
        go.SetActive(false);
    }
}