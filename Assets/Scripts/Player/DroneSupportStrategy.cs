using UnityEngine;

public class DroneSupportStrategy : IShootStrategy
{
    private Transform[] drones;
    public DroneSupportStrategy(Transform[] droneSpawns, ObjectPool dronePool)
    {
        drones = new Transform[droneSpawns.Length];
        for (int i = 0; i < droneSpawns.Length; i++)
        {
            GameObject drone = dronePool.GetObject();
            drone.transform.position = droneSpawns[i].position;
            drone.transform.rotation = droneSpawns[i].rotation;
            drone.SetActive(true);
            drones[i] = drone.transform;
        }
    }
    public void Shoot(Transform firePoint, ObjectPool bulletPool)
    {
        foreach (var drone in drones)
        {
            if (drone != null)
            {
                GameObject bullet = bulletPool.GetObject();
                bullet.transform.position = drone.position;
                bullet.transform.rotation = drone.rotation;
                bullet.SetActive(true);
            }
        }
    }
}
