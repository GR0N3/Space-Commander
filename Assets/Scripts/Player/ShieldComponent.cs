using UnityEngine;

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

    public int CurrentResistance => currentResistance;

    public int MaxResistance => maxResistance;

    public bool IsActive => shieldVisual != null && shieldVisual.activeSelf && currentResistance > 0;

    void Awake()
    {
        entityLife = GetComponent<EntityLife>();
        if (entityLife == null)
        {
            entityLife = GetComponentInParent<EntityLife>();
        }
    }

    public void ActivateShield(int resistance)
    {
        maxResistance = resistance;
        currentResistance = resistance;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true);
        }

        if (Application.isPlaying)
        {
            gameObject.SendMessage("OnShieldActivated", new ShieldResistanceData(currentResistance, maxResistance), SendMessageOptions.DontRequireReceiver);
        }
    }

    public void TakeShieldDamage(int damage)
    {
        if (!IsActive) return;

        currentResistance -= damage;
        currentResistance = Mathf.Max(0, currentResistance);
        if (Application.isPlaying)
        {
            gameObject.SendMessage("OnShieldDamaged", new ShieldResistanceData(currentResistance, maxResistance), SendMessageOptions.DontRequireReceiver);
        }
        if (currentResistance <= 0)
        {
            DeactivateShield();
            if (Application.isPlaying)
            {
                SendMessageUpwards("OnShieldBroken", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public void DeactivateShield()
    {
        currentResistance = 0;

        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }

        if (Application.isPlaying)
        {
            gameObject.SendMessage("OnShieldDeactivated", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void RestoreShield()
    {
        currentResistance = maxResistance;
        if (shieldVisual != null && !shieldVisual.activeSelf)
        {
            shieldVisual.SetActive(true);
        }
    }
}

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
