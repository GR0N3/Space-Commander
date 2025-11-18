using UnityEngine;

[RequireComponent(typeof(EntityLife))]
public class EnemyLife : MonoBehaviour
{
    [Header("Type Object - Life Data")]
    [Tooltip("LifeType que define las propiedades de vida del enemigo. Asigna un ScriptableObject desde el Inspector.")]
    public LifeType enemyLifeType;

    private EntityLife entityLife;

    void Awake()
    {
        entityLife = GetComponent<EntityLife>();
        if (entityLife == null)
        {
            entityLife = gameObject.AddComponent<EntityLife>();
        }
        if (enemyLifeType != null)
        {
            entityLife.lifeType = enemyLifeType;
        }
    }
    public void TakeDamage(int amount)
    {
        if (entityLife != null)
        {
            entityLife.TakeDamage(amount);
        }
    }
    public int CurrentHealth => entityLife != null ? entityLife.CurrentHealth : 0;
    public int MaxHealth => entityLife != null ? entityLife.MaxHealth : 100;
}

