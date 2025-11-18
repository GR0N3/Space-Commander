using UnityEngine;

public class OrbitingDronesStrategy : IShootStrategy
{
    private Transform player;
    private Transform[] drones;
    private float orbitRadius = 5f;
    private int droneCount = 2;
    private float orbitSpeed = 60f; // grados por segundo

    public OrbitingDronesStrategy(Transform player, ObjectPool dronePool)
    {
        this.player = player;
        drones = new Transform[droneCount];
        for (int i = 0; i < droneCount; i++)
        {
            GameObject drone = dronePool.GetObject();
            drone.SetActive(true);
            drones[i] = drone.transform;
        }
    }
    public void Shoot(Transform firePoint, ObjectPool bulletPool)
    {
        foreach (var drone in drones)
        {
            GameObject bullet = bulletPool.GetObject();
            bullet.transform.position = drone.position;
            bullet.transform.rotation = drone.rotation;
            bullet.SetActive(true);
        }
    }
    public void UpdateOrbit(float deltaTime)
    {
        for (int i = 0; i < drones.Length; i++)
        {
            float angle = (360f / droneCount) * i + Time.time * orbitSpeed;
            Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * orbitRadius;
            drones[i].position = player.position + offset;
            drones[i].LookAt(player.position);
        }
    }
}
