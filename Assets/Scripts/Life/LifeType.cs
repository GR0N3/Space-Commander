using UnityEngine;

/// <summary>
/// ScriptableObject que define un tipo de vida usando el patrón Type Object.
/// Cada variación de entidad (jugador, enemigo, etc.) se define como datos, no como nuevas clases.
/// Esto permite agregar nuevos tipos de entidades simplemente creando un nuevo ScriptableObject en el editor.
/// </summary>
[CreateAssetMenu(fileName = "NewLifeType", menuName = "Type Object/LifeType", order = 2)]
public class LifeType : ScriptableObject
{
    [Header("Identificación")]
    [Tooltip("Nombre de la entidad (ej: 'Player', 'BasicEnemy', 'BossEnemy')")]
    public string entityName = "Nueva Entidad";
    
    [Tooltip("Descripción de la entidad (opcional, para documentación)")]
    [TextArea(2, 4)]
    public string description = "";

    [Header("Vida")]
    [Tooltip("Vida máxima de la entidad")]
    [Range(1, 1000)]
    public int maxHealth = 100;

    [Header("Escudo")]
    [Tooltip("Si está activo, la entidad puede tener escudo que absorbe daño")]
    public bool hasShield = false;
    
    [Tooltip("Resistencia del escudo (cuánto daño puede absorber antes de romperse). Se usa cuando el escudo se activa desde LifeType directamente.")]
    [Range(0, 1000)]
    public int shieldHealth = 100;

    [Header("Efectos de Muerte")]
    [Tooltip("Prefab de explosión/efecto visual al morir")]
    public GameObject deathEffectPrefab;
    
    [Tooltip("Sonido de explosión/muerte")]
    public AudioClip deathSound;
    public float deathSoundVolume = 1f;
    
    [Tooltip("Puntos que otorga al ser destruido (0 = no otorga puntos, solo para enemigos)")]
    [Range(0, 10000)]
    public int pointsOnDeath = 0;

    [Header("Efectos Visuales/Sonido (Opcional)")]
    [Tooltip("Sprite o icono de la entidad (para UI)")]
    public Sprite icon;
    
    [Tooltip("Color de la entidad (para UI o efectos)")]
    public Color entityColor = Color.white;

    /// <summary>
    /// Valida que la configuración del LifeType sea coherente
    /// </summary>
    private void OnValidate()
    {
        // Asegurar que maxHealth sea positivo
        maxHealth = Mathf.Max(1, maxHealth);
        
        // Si no tiene escudo, resetear valores relacionados
        if (!hasShield)
        {
            shieldHealth = 0;
        }
        
        // Asegurar que shieldHealth sea positivo si tiene escudo
        if (hasShield)
        {
            shieldHealth = Mathf.Max(1, shieldHealth);
        }
    }
}

