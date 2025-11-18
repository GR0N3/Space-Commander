using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Configuration")]
    [SerializeField] private EnemyFactory factory;
    
    [Header("Spawn Points")]
    [Tooltip("Array de puntos de spawn. Si está vacío, usará la posición del GameObject")]
    [SerializeField] private Transform[] spawnPoints;
    
    [Header("Spawn Settings")]
    [Tooltip("Si está activado, elegirá un spawnpoint aleatorio. Si no, usará el orden del array")]
    [SerializeField] private bool useRandomSpawnPoint = true;
    
    [Tooltip("Si está activado, spawnea automáticamente al iniciar")]
    [SerializeField] private bool spawnOnStart = false;
    
    [Tooltip("Cantidad de enemigos a spawnear al iniciar (si spawnOnStart está activado)")]
    [SerializeField] private int spawnCountOnStart = 1;
    
    [Header("Input")]
    [Tooltip("Tecla para spawnear un enemigo manualmente")]
    [SerializeField] private KeyCode spawnKey = KeyCode.E;

    void Start()
    {
        if (spawnOnStart)
        {
            for (int i = 0; i < spawnCountOnStart; i++)
            {
                SpawnEnemy();
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(spawnKey))
        {
            SpawnEnemy();
        }
    }
    
    public void SpawnEnemy()
    {
        Vector3 spawnPosition = GetSpawnPosition();
        factory.CreateRandomEnemy(spawnPosition);
    }
    
    public void SpawnEnemy(string enemyId)
    {
        Vector3 spawnPosition = GetSpawnPosition();
        factory.CreateEnemy(enemyId, spawnPosition);
    }
    
    private Vector3 GetSpawnPosition()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return transform.position;
        }
        Transform selectedSpawnPoint;
        if (useRandomSpawnPoint)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            selectedSpawnPoint = spawnPoints[randomIndex];
        }
        else
        {
            selectedSpawnPoint = spawnPoints[0];
        }
        if (selectedSpawnPoint == null)
        {
            return transform.position;
        }
        return selectedSpawnPoint.position;
    }
    
    public Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return null;
        }
        int randomIndex = Random.Range(0, spawnPoints.Length);
        return spawnPoints[randomIndex];
    }
    
    public Transform[] GetAllSpawnPoints()
    {
        return spawnPoints;
    }
}
