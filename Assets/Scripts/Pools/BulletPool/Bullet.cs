using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]

public class Bullet : MonoBehaviour
{
    [Header("Flyweight Data")]
    [SerializeField] private BulletData bulletData; // Estado intrínseco compartido

    [Header("Extrinsic State")]
    [Tooltip("Tag del jugador para detectar colisiones (fallback)")]
    [SerializeField] private string playerTag = "Player";

    private Coroutine lifetimeCoroutine;
    private Collider col;
    private Rigidbody rb;
    
    /// <summary>
    /// Inicializa la bala con los datos del ScriptableObject
    /// </summary>
    public void Initialize(BulletData data)
    {
        if (data == null) return;
        bulletData = data;
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
        }
        lifetimeCoroutine = StartCoroutine(LifetimeCoroutine());
    }

    private void Awake()
    {
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        if (col != null) col.isTrigger = true;
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    private void OnEnable()
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
        }
        lifetimeCoroutine = StartCoroutine(LifetimeCoroutine());
    }

    private void OnDisable()
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }
    }

    private void Update()
    {
        if (bulletData != null)
        {
            transform.Translate(Vector3.forward * bulletData.speed * Time.deltaTime);
        }
    }

    private IEnumerator LifetimeCoroutine()
    {
        if (bulletData != null)
        {
            yield return new WaitForSeconds(bulletData.lifeTime);
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }
        Deactivate();
    }

    private void Deactivate()
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }
        gameObject.SetActive(false);
    }

    private string GetExpectedTargetTag()
    {
        if (bulletData == null) return string.IsNullOrEmpty(playerTag) ? "Player" : playerTag;
        if (bulletData.ownerTag == "Enemy") return "Player";
        if (bulletData.ownerTag == "Player") return "Enemy";
        return string.IsNullOrEmpty(playerTag) ? "Player" : playerTag;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (bulletData != null)
        {
            
            string targetTag = GetExpectedTargetTag();
            // Si la bala es de enemigo y choca con el jugador
            if (bulletData.ownerTag == "Enemy" && other.CompareTag(targetTag))
            {
                // Usar EntityLife si está disponible, sino usar PlayerHealth para compatibilidad
                EntityLife entityLife = other.GetComponent<EntityLife>();
                if (entityLife != null)
                {
                    entityLife.TakeDamage(bulletData.damage);
                }
                else
                {
                    PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(bulletData.damage);
                    }
                }
                if (bulletData.hitSound != null)
                {
                    AudioSource.PlayClipAtPoint(bulletData.hitSound, transform.position, bulletData.hitVolume);
                }
                Deactivate();
            }
            // Si la bala es del jugador y choca con un enemigo
            else if (bulletData.ownerTag == "Player" && other.CompareTag(targetTag))
            {
                // Usar EntityLife si está disponible
                EntityLife entityLife = other.GetComponent<EntityLife>();
                if (entityLife != null)
                {
                    entityLife.TakeDamage(bulletData.damage);
                }
                else
                {
                    // Fallback: usar EnemyLife si existe
                    EnemyLife enemyLife = other.GetComponent<EnemyLife>();
                    if (enemyLife != null)
                    {
                        enemyLife.TakeDamage(bulletData.damage);
                    }
                    else
                    {
                        Destroy(other.gameObject);
                    }
                }
                if (bulletData.hitSound != null)
                {
                    AudioSource.PlayClipAtPoint(bulletData.hitSound, transform.position, bulletData.hitVolume);
                }
                Deactivate();
            }
        }
        else { }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var other = collision.collider;
        if (bulletData != null)
        {
            string targetTag = GetExpectedTargetTag();
            if (bulletData.ownerTag == "Enemy" && other.CompareTag(targetTag))
            {
                var entityLife = other.GetComponent<EntityLife>();
                if (entityLife != null)
                {
                    entityLife.TakeDamage(bulletData.damage);
                }
                else
                {
                    var playerHealth = other.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(bulletData.damage);
                    }
                }
                if (bulletData.hitSound != null)
                {
                    AudioSource.PlayClipAtPoint(bulletData.hitSound, transform.position, bulletData.hitVolume);
                }
                Deactivate();
            }
            else if (bulletData.ownerTag == "Player" && other.CompareTag(targetTag))
            {
                var entityLife = other.GetComponent<EntityLife>();
                if (entityLife != null)
                {
                    entityLife.TakeDamage(bulletData.damage);
                }
                else
                {
                    var enemyLife = other.GetComponent<EnemyLife>();
                    if (enemyLife != null)
                    {
                        enemyLife.TakeDamage(bulletData.damage);
                    }
                    else
                    {
                        Destroy(other.gameObject);
                    }
                }
                if (bulletData.hitSound != null)
                {
                    AudioSource.PlayClipAtPoint(bulletData.hitSound, transform.position, bulletData.hitVolume);
                }
                Deactivate();
            }
        }
        else { }
    }

    public string OwnerTag
    {
        get { return bulletData != null ? bulletData.ownerTag : string.Empty; }
    }
}
