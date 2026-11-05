using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject objPrefab;
    [SerializeField] private int poolSize = 20;
    private List<GameObject> pool = new List<GameObject>();

    public GameObject ObjPrefab { get => objPrefab; set => objPrefab = value; }
    public int PoolSize { get => poolSize; set => poolSize = value; }
    public IReadOnlyList<GameObject> Pool => pool;

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(objPrefab);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    public GameObject GetObject()
    {
        foreach (var obj in pool)
        {
            if (!obj.activeInHierarchy)
                return obj;
        }
        GameObject newObj = Instantiate(objPrefab);
        newObj.SetActive(false);
        pool.Add(newObj);
        return newObj;
    }
}
