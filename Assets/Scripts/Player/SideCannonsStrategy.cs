using UnityEngine;

public class SideCannonsStrategy : IShootStrategy
{
    private float sideOffset = 1.5f;
    public void Shoot(Transform firePoint, ObjectPool bulletPool)
    {
        Vector3 leftPos = firePoint.position + firePoint.right * -sideOffset;
        SpawnBullet(leftPos, firePoint.rotation, bulletPool);
        Vector3 rightPos = firePoint.position + firePoint.right * sideOffset;
        SpawnBullet(rightPos, firePoint.rotation, bulletPool);
    }
    private void SpawnBullet(Vector3 pos, Quaternion rot, ObjectPool pool)
    {
        GameObject bullet = pool.GetObject();
        bullet.transform.position = pos;
        bullet.transform.rotation = rot;
        bullet.SetActive(true);
    }
}
