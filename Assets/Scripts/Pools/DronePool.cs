using System.Collections.Generic;
using UnityEngine;

public class DronePool : MonoBehaviour
{
    [SerializeField] private GameObject dronePrefab;
    private Queue<Drone> pool = new Queue<Drone>();

    public Drone SpawnDrone(Vector3 position)
    {
        Drone drone = pool.Count > 0 ? pool.Dequeue() : Instantiate(dronePrefab).GetComponent<Drone>();
        drone.transform.position = position;
        drone.gameObject.SetActive(true);
        return drone;
    }

    public void ReturnDrone(Drone drone)
    {
        drone.gameObject.SetActive(false);
        pool.Enqueue(drone);
    }
}
