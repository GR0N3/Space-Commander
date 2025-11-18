using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int poolSize = 20;

    private List<GameObject> pool;
    private static BulletPool instance;

    public static BulletPool Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Object.FindFirstObjectByType<BulletPool>();
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
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            pool.Add(bullet);
        }
    }

    public GameObject GetBullet()
    {
        foreach (GameObject bullet in pool)
        {
            if (!bullet.activeInHierarchy)
            {
                return bullet;
            }
        }

        GameObject newBullet = Instantiate(bulletPrefab);
        newBullet.SetActive(false);
        pool.Add(newBullet);
        return newBullet;
    }
    public GameObject GetBullet(Vector3 position, Quaternion rotation)
    {
        GameObject bullet = GetBullet();
        if (bullet != null)
        {
            bullet.transform.position = position;
            bullet.transform.rotation = rotation;
            bullet.SetActive(true);
        }
        return bullet;
    }
}
