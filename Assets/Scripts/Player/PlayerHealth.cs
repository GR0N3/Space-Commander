using UnityEngine;

/// <summary>
/// Componente de salud del jugador que usa el sistema centralizado de vida (EntityLife).
/// Mantiene compatibilidad con el código existente mientras usa el nuevo sistema.
/// </summary>
[RequireComponent(typeof(EntityLife))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Type Object - Life Data")]
    [Tooltip("LifeType que define las propiedades de vida del jugador. Asigna un ScriptableObject desde el Inspector.")]
    public LifeType playerLifeType;

    private EntityLife entityLife;

    // Propiedades de compatibilidad hacia atrás
    public int maxHealth => entityLife != null ? entityLife.MaxHealth : 100;
    public int currentHealth => entityLife != null ? entityLife.CurrentHealth : 100;

    void Awake()
    {
        // Obtener o agregar EntityLife
        entityLife = GetComponent<EntityLife>();
        if (entityLife == null)
        {
            entityLife = gameObject.AddComponent<EntityLife>();
        }

        // Asignar LifeType si está configurado
        if (playerLifeType != null)
        {
            entityLife.lifeType = playerLifeType;
        }
    }

    /// <summary>
    /// Aplica daño al jugador (compatibilidad hacia atrás)
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (entityLife != null)
        {
            entityLife.TakeDamage(amount);
        }
    }

    /// <summary>
    /// Cura al jugador (compatibilidad hacia atrás)
    /// </summary>
    public void Heal(int amount)
    {
        if (entityLife != null)
        {
            entityLife.Heal(amount);
        }
    }

    void OnEnable()
    {
        // Escuchar eventos de EntityLife para mantener compatibilidad
        if (entityLife != null)
        {
            // El EntityLife ya maneja los mensajes, pero podemos agregar lógica adicional aquí si es necesario
        }
    }
}
