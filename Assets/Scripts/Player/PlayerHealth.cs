using UnityEngine;

[RequireComponent(typeof(EntityLife))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Type Object - Life Data")]
    [Tooltip("LifeType que define las propiedades de vida del jugador. Asigna un ScriptableObject desde el Inspector.")]
    public LifeType playerLifeType;

    private EntityLife entityLife;
    [SerializeField] private bool blinkOnDamage = true;
    [SerializeField] private int blinkCount = 3;
    [SerializeField] private float blinkInterval = 0.1f;
    [SerializeField] private Renderer[] renderers;
    private int prevHealth;

    public int maxHealth => entityLife != null ? entityLife.MaxHealth : 100;
    public int currentHealth => entityLife != null ? entityLife.CurrentHealth : 100;

    void Awake()
    {
        entityLife = GetComponent<EntityLife>();
        if (entityLife == null)
        {
            entityLife = gameObject.AddComponent<EntityLife>();
        }

        if (playerLifeType != null)
        {
            entityLife.lifeType = playerLifeType;
        }
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>(true);
        }
        prevHealth = currentHealth;
    }

    public void TakeDamage(int amount)
    {
        if (entityLife != null)
        {
            entityLife.TakeDamage(amount);
        }
    }

    public void Heal(int amount)
    {
        if (entityLife != null)
        {
            entityLife.Heal(amount);
        }
    }

    void OnEnable()
    {
        if (entityLife != null)
        {
        }
    }

    void OnHealthChanged(HealthChangedData data)
    {
        if (!blinkOnDamage) { prevHealth = data.currentHealth; return; }
        if (data.currentHealth < prevHealth)
        {
            StopAllCoroutines();
            StartCoroutine(BlinkCoroutine());
        }
        prevHealth = data.currentHealth;
    }

    System.Collections.IEnumerator BlinkCoroutine()
    {
        if (renderers == null || renderers.Length == 0) yield break;
        for (int i = 0; i < blinkCount; i++)
        {
            SetRenderersEnabled(false);
            yield return new WaitForSeconds(blinkInterval);
            SetRenderersEnabled(true);
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    void SetRenderersEnabled(bool enabled)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) renderers[i].enabled = enabled;
        }
    }
}
