using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]

public class Bullet : MonoBehaviour
{
    [Header("Flyweight Data")]
    [SerializeField] private BulletData bulletData;

    [Header("Extrinsic State")]
    [Tooltip("Tag del jugador para detectar colisiones (fallback)")]
    [SerializeField] private string playerTag = "Player";

    private Coroutine lifetimeCoroutine;
    private Collider col;
    private Rigidbody rb;
    [SerializeField] private LayerMask hitMask = ~0;
    private Vector3 lastPos;
    
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
        lastPos = transform.position;
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
            float dist = bulletData.speed * Time.deltaTime;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, dist, hitMask))
            {
                if (IsValidTarget(hit.collider))
                {
                    ApplyDamageTo(hit.collider);
                    return;
                }
            }
            transform.Translate(Vector3.forward * dist);
            lastPos = transform.position;
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
        if (bulletData == null) return;
        if (!IsValidTarget(other)) return;
        ApplyDamageTo(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var other = collision.collider;
        if (bulletData == null) return;
        if (!IsValidTarget(other)) return;
        ApplyDamageTo(other);
    }

    private bool IsValidTarget(Collider other)
    {
        if (bulletData.ownerTag == "Enemy")
        {
            return HasTag(other, "Player");
        }
        if (bulletData.ownerTag == "Player")
        {
            if (HasTag(other, "Enemy")) return true;
            if (HasTag(other, "Boss")) return true;
            return false;
        }
        string targetTag = GetExpectedTargetTag();
        return HasTag(other, targetTag);
    }

    private bool HasTag(Collider other, string tag)
    {
        if (other == null) return false;
        if (other.CompareTag(tag)) return true;
        Transform t = other.transform;
        if (t != null)
        {
            if (t.parent != null && t.parent.CompareTag(tag)) return true;
            if (t.root != null && t.root.CompareTag(tag)) return true;
        }
        return false;
    }

    private void ApplyDamageTo(Collider other)
    {
        EntityLife entityLife = other.GetComponent<EntityLife>();
        if (entityLife == null)
        {
            entityLife = other.GetComponentInParent<EntityLife>();
        }
        if (entityLife != null)
        {
            entityLife.TakeDamage(bulletData.damage);
            var bossHit = other.GetComponent<Boss>() != null || other.GetComponentInParent<Boss>() != null;
            if (bossHit && bulletData != null && bulletData.ownerTag == "Player")
            {
                var service = ScoreManager.Instance as IScoreService;
                if (service != null) service.AddScore(250); else ScoreManager.Instance.AddScore(250);
            }
        }
        else
        {
            if (bulletData.ownerTag == "Enemy")
            {
                var playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null) playerHealth.TakeDamage(bulletData.damage);
            }
            else if (bulletData.ownerTag == "Player")
            {
                var enemyLife = other.GetComponent<EnemyLife>();
                if (enemyLife != null) enemyLife.TakeDamage(bulletData.damage);
                var bossHit = other.GetComponent<Boss>() != null || other.GetComponentInParent<Boss>() != null;
                if (bossHit)
                {
                    var service = ScoreManager.Instance as IScoreService;
                    if (service != null) service.AddScore(250); else ScoreManager.Instance.AddScore(250);
                }
            }
        }
        if (bulletData.hitSound != null)
        {
            AudioSource.PlayClipAtPoint(bulletData.hitSound, transform.position, bulletData.hitVolume);
        }
        Deactivate();
    }

    public string OwnerTag
    {
        get { return bulletData != null ? bulletData.ownerTag : string.Empty; }
    }
}
