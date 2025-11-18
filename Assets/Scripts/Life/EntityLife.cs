using UnityEngine;

public class EntityLife : MonoBehaviour
{
    [Header("Type Object - Life Data")]
    [Tooltip("LifeType que define las propiedades de vida de esta entidad. Asigna un ScriptableObject desde el Inspector.")]
    public LifeType lifeType;

    [Header("Facade Reference")]
    [Tooltip("LifeFacade que manejará la muerte. Si es null, se buscará automáticamente o se usará el singleton.")]
    public LifeFacade lifeFacade;

    [Header("Estado Actual")]
    [Tooltip("Vida actual de la entidad (se inicializa desde LifeType)")]
    [SerializeField] private int currentHealth;

    [Tooltip("Escudo actual activo (solo si LifeType.hasShield es true)")]
    [SerializeField] private bool shieldActive = false;

    [Tooltip("Vida restante del escudo")]
    [SerializeField] private int currentShieldHealth = 0;

    private Coroutine shieldCoroutine;

    public int CurrentHealth => currentHealth;

    public int MaxHealth => lifeType != null ? lifeType.maxHealth : 100;

    public bool IsShieldActive => shieldActive;

    public int CurrentShieldHealth => currentShieldHealth;

    void Awake()
    {
        if (lifeType != null)
        {
            currentHealth = lifeType.maxHealth;
            if (lifeType.hasShield)
            {
                currentShieldHealth = lifeType.shieldHealth;
            }
        }
        else
        {
            currentHealth = 100;
        }
        if (lifeFacade == null)
        {
            lifeFacade = LifeFacade.Instance;
            if (lifeFacade == null)
            {
                lifeFacade = Object.FindFirstObjectByType<LifeFacade>();
            }
        }
    }

    void Start()
    {
        gameObject.SendMessage("OnHealthChanged", new HealthChangedData(currentHealth, MaxHealth), SendMessageOptions.DontRequireReceiver);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        ShieldComponent powerUpShield = null;
        if (transform.parent != null)
        {
            powerUpShield = transform.parent.GetComponentInChildren<ShieldComponent>();
        }
        if (powerUpShield == null)
        {
            powerUpShield = GetComponentInChildren<ShieldComponent>();
        }
        if (powerUpShield != null && powerUpShield.IsActive)
        {
            int damageToPowerUpShield = Mathf.Min(amount, powerUpShield.CurrentResistance);
            powerUpShield.TakeShieldDamage(damageToPowerUpShield);
            amount -= damageToPowerUpShield;
            if (amount <= 0) return;
        }
        if (shieldActive && currentShieldHealth > 0)
        {
            int damageToShield = Mathf.Min(amount, currentShieldHealth);
            currentShieldHealth -= damageToShield;
            amount -= damageToShield;
            if (currentShieldHealth <= 0)
            {
                shieldActive = false;
                currentShieldHealth = 0;
                HandleShieldBrokenInternal();
            }
            if (amount > 0)
            {
                ApplyDamageToHealth(amount);
            }
        }
        else
        {
            ApplyDamageToHealth(amount);
        }
    }

    private void ApplyDamageToHealth(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        gameObject.SendMessage("OnHealthChanged", new HealthChangedData(currentHealth, MaxHealth), SendMessageOptions.DontRequireReceiver);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHealth += amount;
        currentHealth = Mathf.Min(MaxHealth, currentHealth);
        gameObject.SendMessage("OnHealthChanged", new HealthChangedData(currentHealth, MaxHealth), SendMessageOptions.DontRequireReceiver);
    }

    public void ActivateShield(int resistance = 0)
    {
        if (lifeType == null || !lifeType.hasShield)
        {
            return;
        }

        shieldActive = true;
        if (resistance > 0)
        {
            currentShieldHealth = resistance;
        }
        else if (lifeType.shieldHealth > 0)
        {
            currentShieldHealth = lifeType.shieldHealth;
        }
        else
        {
            currentShieldHealth = 100;
        }

        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
            shieldCoroutine = null;
        }
        gameObject.SendMessage("OnShieldActivated", SendMessageOptions.DontRequireReceiver);
    }

    public void DeactivateShield()
    {
        shieldActive = false;
        currentShieldHealth = 0;

        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
            shieldCoroutine = null;
        }
        gameObject.SendMessage("OnShieldDeactivated", SendMessageOptions.DontRequireReceiver);
    }


    private void HandleShieldBrokenInternal()
    {
        gameObject.SendMessage("OnShieldBroken", SendMessageOptions.DontRequireReceiver);
    }

    private void Die()
    {
        if (lifeType == null)
        {
            if (gameObject.CompareTag("Player"))
            {
                gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
            return;
        }
        if (lifeFacade != null)
        {
            lifeFacade.HandleDeath(lifeType, transform.position, gameObject);
        }
        else
        {
            LifeFacade.HandleEntityDeath(lifeType, transform.position, gameObject);
        }
        gameObject.SendMessage("OnEntityDeath", SendMessageOptions.DontRequireReceiver);
    }

    void OnDestroy()
    {
        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
        }
    }
}

public struct HealthChangedData
{
    public int currentHealth;
    public int maxHealth;

    public HealthChangedData(int current, int max)
    {
        currentHealth = current;
        maxHealth = max;
    }
}

