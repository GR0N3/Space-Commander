using UnityEngine;

public class RespawnOnDeath : MonoBehaviour
{
    public MonoBehaviour spawnerBehaviour;
    private ISpawner spawner;
    public AreaPowerUpSpawner powerUpSpawner;
    public float powerUpDropChance = 0.3f;

    void Awake()
    {
        spawner = spawnerBehaviour as ISpawner;
    }

    void OnEntityDeath()
    {
        if (spawner != null)
        {
            spawner.SpawnEnemy();
        }
        if (powerUpSpawner != null)
        {
            if (Random.value < powerUpDropChance)
            {
                powerUpSpawner.SpawnPowerUp();
            }
        }
    }
}
