using System.Collections.Generic;
using UnityEngine;

public class PowerUpPool : MonoBehaviour
{
    public GameObject prefab;
    public int poolSize = 10;
    private List<GameObject> pool = new List<GameObject>();

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var go = Instantiate(prefab);
            go.SetActive(false);
            pool.Add(go);
        }
    }

    public GameObject Get()
    {
        foreach (var go in pool)
        {
            if (go == null) continue;
            if (!go.activeInHierarchy) return go;
        }
        pool.RemoveAll(g => g == null);
        var n = Instantiate(prefab);
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
