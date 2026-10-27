using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestor de Frustum Culling para desactivar el renderizado y efectos visuales
/// de los elementos que se encuentran fuera de la pantalla, mejorando el rendimiento.
/// </summary>
public class ScreenCullingManager : MonoBehaviour
{
    private static ScreenCullingManager instance;
    public static ScreenCullingManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Object.FindFirstObjectByType<ScreenCullingManager>();
            }
            return instance;
        }
    }

    [Header("Configuración de Culling")]
    [Tooltip("Cámara utilizada para calcular el frustum visible. Si es null, usa Camera.main")]
    public Camera targetCamera;

    [Tooltip("Margen extra alrededor del objeto (en unidades del mundo) para evitar popping visual al entrar en pantalla")]
    public float cullingMargin = 4f;

    [Tooltip("Intervalo en segundos para auto-registrar nuevos objetos dinámicos en la escena")]
    public float autoDiscoveryInterval = 0.5f;

    private Plane[] frustumPlanes = new Plane[6];
    private readonly List<CullableObject> registeredObjects = new List<CullableObject>(128);
    private readonly HashSet<CullableObject> registeredSet = new HashSet<CullableObject>();
    private float nextDiscoveryTime = 0f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
            return;
        }

        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
            if (targetCamera == null) targetCamera = Camera.main;
        }
    }

    void Start()
    {
        DiscoverSceneCullables();
    }

    void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }

        if (Time.time >= nextDiscoveryTime)
        {
            nextDiscoveryTime = Time.time + autoDiscoveryInterval;
            DiscoverSceneCullables();
        }

        GeometryUtility.CalculateFrustumPlanes(targetCamera, frustumPlanes);

        for (int i = registeredObjects.Count - 1; i >= 0; i--)
        {
            CullableObject obj = registeredObjects[i];
            if (obj == null)
            {
                registeredObjects.RemoveAt(i);
                continue;
            }
            if (!obj.gameObject.activeInHierarchy)
            {
                continue;
            }

            Bounds b = obj.GetCullingBounds(cullingMargin);
            bool visible = GeometryUtility.TestPlanesAABB(frustumPlanes, b);
            obj.SetCulledState(!visible);
        }
    }

    public void Register(CullableObject obj)
    {
        if (obj == null) return;
        if (registeredSet.Add(obj))
        {
            registeredObjects.Add(obj);
        }
    }

    public void Unregister(CullableObject obj)
    {
        if (obj == null) return;
        if (registeredSet.Remove(obj))
        {
            registeredObjects.Remove(obj);
        }
    }

    public void DiscoverSceneCullables()
    {
        // Auto-agregar CullableObject a enemigos, power-ups, balas y jefe si no lo tienen
        var enemies = Object.FindObjectsByType<EnemyController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < enemies.Length; i++)
        {
            EnsureCullable(enemies[i].gameObject);
        }

        var powerUps = Object.FindObjectsByType<PowerUp>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < powerUps.Length; i++)
        {
            EnsureCullable(powerUps[i].gameObject);
        }

        var bullets = Object.FindObjectsByType<Bullet>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < bullets.Length; i++)
        {
            EnsureCullable(bullets[i].gameObject);
        }

        var bosses = Object.FindObjectsByType<Boss>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < bosses.Length; i++)
        {
            EnsureCullable(bosses[i].gameObject);
        }
    }

    private void EnsureCullable(GameObject go)
    {
        if (go == null || go.CompareTag("Player")) return;
        CullableObject cullable = go.GetComponent<CullableObject>();
        if (cullable == null)
        {
            cullable = go.AddComponent<CullableObject>();
        }
        Register(cullable);
    }
}
