#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Editor helper: crea un BulletData por defecto (si no existe) y lo asigna a todos los EnemyController en la escena
/// que no tengan `enemyBulletData` configurado.
/// Menu: Tools/Assign Default BulletData to Enemies
/// </summary>
public class AssignDefaultEnemyBulletData : MonoBehaviour
{
    private const string defaultAssetPath = "Assets/Resources/DefaultEnemyBulletData.asset";

    [MenuItem("Tools/Assign Default BulletData to Enemies")]
    public static void Assign()
    {
        // Asegurar que la carpeta Resources existe
        string resourcesFolder = Path.Combine(Application.dataPath, "Resources");
        if (!Directory.Exists(resourcesFolder))
        {
            Directory.CreateDirectory(resourcesFolder);
            AssetDatabase.Refresh();
        }

        BulletData data = AssetDatabase.LoadAssetAtPath<BulletData>(defaultAssetPath);
        if (data == null)
        {
            // Crear un asset por defecto
            data = ScriptableObject.CreateInstance<BulletData>();
            data.speed = 8f;
            data.lifeTime = 2f;
            data.damage = 1;
            data.ownerTag = "Enemy";
            AssetDatabase.CreateAsset(data, defaultAssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log("AssignDefaultEnemyBulletData: creado DefaultEnemyBulletData.asset en Resources.");
        }

        // Encontrar todos los EnemyController en la escena abierta
        var enemies = GameObject.FindObjectsOfType<UnityEngine.MonoBehaviour>(true);
        int assigned = 0;
        foreach (var mb in enemies)
        {
            if (mb == null) continue;
            var type = mb.GetType();
            if (type.Name == "EnemyController")
            {
                // Usar reflexión para leer/assign el campo enemyBulletData
                var field = type.GetField("enemyBulletData");
                if (field != null)
                {
                    var current = field.GetValue(mb) as BulletData;
                    if (current == null)
                    {
                        field.SetValue(mb, data);
                        EditorUtility.SetDirty(mb);
                        assigned++;
                    }
                }
            }
        }

        if (assigned > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"AssignDefaultEnemyBulletData: asignado DefaultEnemyBulletData a {assigned} EnemyController(s) en la escena.");
        }
        else
        {
            Debug.Log("AssignDefaultEnemyBulletData: no se encontraron EnemyController sin BulletData en la escena.");
        }
    }
}
#endif
