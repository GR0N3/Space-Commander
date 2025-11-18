using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyEntry
{
    public string id;
    public GameObject prefab;
}

public class EnemyFactory : MonoBehaviour
{
    [SerializeField] private List<EnemyEntry> enemyList;

    public GameObject CreateEnemy(string id, Vector3 position)
    {
        foreach (var entry in enemyList)
        {
            if (entry.id == id)
            {
                if (entry.prefab == null) { return null; }
                return Instantiate(entry.prefab, position, Quaternion.identity);
            }
        }
        return null;
    }

    public GameObject CreateRandomEnemy(Vector3 position)
    {
        if (enemyList == null || enemyList.Count == 0)
        {
            return null;
        }
        var valid = new List<EnemyEntry>();
        for (int i = 0; i < enemyList.Count; i++)
        {
            var e = enemyList[i];
            if (e != null && e.prefab != null) valid.Add(e);
        }
        if (valid.Count == 0)
        {
            return null;
        }
        int index = Random.Range(0, valid.Count);
        return Instantiate(valid[index].prefab, position, Quaternion.identity);
    }
}
