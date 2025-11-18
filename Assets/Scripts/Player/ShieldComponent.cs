using UnityEngine;

/// <summary>
/// Componente que gestiona el escudo visual del jugador con sistema de resistencia.
/// Se integra con EntityLife para manejar la resistencia del escudo.
/// </summary>
public class ShieldComponent : MonoBehaviour
{
    [Header("Shield Visual")]
    [Tooltip("GameObject visual del escudo (Shell_1 o Shell_2)")]
    public GameObject shieldVisual;

    [Header("Shield Stats")]
    [Tooltip("Resistencia máxima del escudo")]
    [SerializeField] private int maxResistance = 100;

    [Tooltip("Resistencia actual del escudo")]
    [SerializeField] private int currentResistance = 0;

    private EntityLife entityLife;

    /// <summary>
    /// Obtiene la resistencia actual del escudo
    /// </summary>
    public int CurrentResistance => currentResistance;

    /// <summary>
    /// Obtiene la resistencia máxima del escudo
    /// </summary>
    public int MaxResistance => maxResistance;

    /// <summary>
    /// Obtiene si el escudo está activo
    /// </summary>
    public bool IsActive => shieldVisual != null && shieldVisual.activeSelf && currentResistance > 0;

    void Awake()
    {
        // Buscar EntityLife en el mismo GameObject o en el padre
        entityLife = GetComponent<EntityLife>();
        if (entityLife == null)
        {
            entityLife = GetComponentInParent<EntityLife>();
        }
    }

    /// <summary>
    /// Activa el escudo con la resistencia especificada (desde power-up)
    /// </summary>
    public void ActivateShield(int resistance)
    {
        maxResistance = resistance;
        currentResistance = resistance;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
        }

        // NO activamos el escudo en EntityLife aquí porque el escudo de power-up
        // funciona independientemente. EntityLife manejará el daño cuando se reciba.

        // Enviar mensaje para actualizar UI
        gameObject.SendMessage("OnShieldActivated", new ShieldResistanceData(currentResistance, maxResistance), SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// Aplica daño al escudo (reduce resistencia)
    /// </summary>
    public void TakeShieldDamage(int damage)
    {
        if (!IsActive) return;

        currentResistance -= damage;
        currentResistance = Mathf.Max(0, currentResistance);

        // Enviar mensaje para actualizar UI
        gameObject.SendMessage("OnShieldDamaged", new ShieldResistanceData(currentResistance, maxResistance), SendMessageOptions.DontRequireReceiver);

        // Si el escudo se rompió
        if (currentResistance <= 0)
        {
            DeactivateShield();
        }
    }

    /// <summary>
    /// Desactiva el escudo
    /// </summary>
    public void DeactivateShield()
    {
        currentResistance = 0;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }

        // Enviar mensaje para actualizar UI
        gameObject.SendMessage("OnShieldDeactivated", SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// Restaura el escudo a su resistencia máxima
    /// </summary>
    public void RestoreShield()
    {
        currentResistance = maxResistance;
        if (shieldVisual != null && !shieldVisual.activeSelf)
        {
            shieldVisual.SetActive(true);
        }
    }
}

/// <summary>
/// Estructura de datos para notificar cambios de resistencia del escudo a la UI
/// </summary>
public struct ShieldResistanceData
{
    public int currentResistance;
    public int maxResistance;

    public ShieldResistanceData(int current, int max)
    {
        currentResistance = current;
        maxResistance = max;
    }
}

