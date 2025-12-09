using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("Prefab Base")]
    [Tooltip("Prefab base del power-up (debe tener el componente PowerUp)")]
    public GameObject powerUpPrefab;

    [Header("Type Object - Power-Up Types")]
    [Tooltip("Lista de PowerUpType ScriptableObjects disponibles para spawnear aleatoriamente")]
    public PowerUpType[] availablePowerUpTypes;

    [Header("Spawn Settings")]
    [Tooltip("Si no se asigna un playerTransform, se buscará por tag 'Player' para posicionar el spawn delante del jugador.")]
    public Transform playerTransform;

    [Tooltip("Distancia delante del player donde aparecerá el powerup")] 
    public float forwardOffset = 3f;

    [Tooltip("Altura relativa al player donde aparecerá el powerup")] 
    public float upOffset = 1f;

    [Header("Score-Triggered Spawns")]
    [Tooltip("Activa spawns automáticos basados en la puntuación")] public bool scoreTriggeredSpawns = true;
    [Tooltip("Puntos necesarios por spawn")] public int pointsPerSpawn = 500;
    [Tooltip("Probabilidad de spawn por umbral")] [Range(0f,1f)] public float spawnChance = 0.5f;
    private int nextScoreThreshold;

    void Start()
    {
        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }
        nextScoreThreshold = Mathf.Max(1, pointsPerSpawn);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SpawnRandomPowerUp();
        }
        if (scoreTriggeredSpawns)
        {
            var sm = ScoreManager.Instance;
            if (sm != null)
            {
                int s = sm.Score;
                while (s >= nextScoreThreshold)
                {
                    if (Random.value < spawnChance)
                    {
                        SpawnRandomPowerUp();
                    }
                    nextScoreThreshold += Mathf.Max(1, pointsPerSpawn);
                }
            }
        }
    }

    public void SpawnRandomPowerUp()
    {
        if (availablePowerUpTypes == null || availablePowerUpTypes.Length == 0)
        {
            return;
        }

        PowerUpType randomType = availablePowerUpTypes[Random.Range(0, availablePowerUpTypes.Length)];
        SpawnPowerUp(randomType);
    }

    public GameObject SpawnPowerUp(PowerUpType powerUpType)
    {
        if (powerUpType == null)
        {
            return null;
        }

        GameObject source = powerUpPrefab;

        if (source == null)
        {
            source = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            source.name = $"TempPowerUp_{powerUpType.powerUpName}";
            var col = source.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
            var pcomp = source.GetComponent<PowerUp>();
            if (pcomp == null)
            {
                pcomp = source.AddComponent<PowerUp>();
            }
            source.hideFlags = HideFlags.DontSave;
        }

        Vector3 spawnPos = transform.position;
        Quaternion spawnRot = Quaternion.identity;

        if (playerTransform != null)
        {
            spawnPos = playerTransform.position + playerTransform.forward * forwardOffset + Vector3.up * upOffset;
            spawnRot = Quaternion.identity;
        }

        GameObject go = Instantiate(source, spawnPos, spawnRot);

        PowerUp p = go.GetComponent<PowerUp>();
        if (p == null)
        {
            p = go.AddComponent<PowerUp>();
        }

        p.powerUpType = powerUpType;

        return go;
    }
}
