using System.Collections.Generic;
using UnityEngine;

public class BossBulletPool : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int poolSize = 30;

    private List<GameObject> pool;
    private static BossBulletPool instance;

    public static BossBulletPool Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Object.FindFirstObjectByType<BossBulletPool>();
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null) instance = this;
        pool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            var bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            pool.Add(bullet);
        }
    }

    public GameObject GetBullet()
    {
        foreach (var bullet in pool)
        {
            if (!bullet.activeInHierarchy)
            {
                return bullet;
            }
        }
        var newBullet = Instantiate(bulletPrefab);
        newBullet.SetActive(false);
        pool.Add(newBullet);
        return newBullet;
    }

    public GameObject GetBullet(Vector3 position, Quaternion rotation)
    {
        var bullet = GetBullet();
        bullet.transform.position = position;
        bullet.transform.rotation = rotation;
        bullet.SetActive(true);
        return bullet;
    }
}
