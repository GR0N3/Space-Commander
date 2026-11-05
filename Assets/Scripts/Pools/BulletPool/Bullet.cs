using System.Collections;
using System.Collections.Generic;
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

    // Seguimiento dirigido con pequeño delay (para proyectiles del Boss)
    private bool homingEnabled = false;
    private Transform homingTarget;
    private float homingTrackingDelay = 0.16f;
    private float homingTurnSpeed = 95f;
    private float homingMaxDuration = 1.4f;
    private float homingDisengageDistance = 4.5f;
    private float homingElapsed = 0f;
    private bool homingDisengaged = false;

    private struct TargetSample
    {
        public float time;
        public Vector3 position;
    }
    private readonly Queue<TargetSample> targetHistory = new Queue<TargetSample>(32);
    private Vector3 currentDelayedTargetPos;
    
    public void Initialize(BulletData data)
    {
        if (data == null) return;
        bulletData = data;
        homingEnabled = false;
        homingTarget = null;
        targetHistory.Clear();
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }
        if (Application.isPlaying && gameObject.activeInHierarchy)
        {
            lifetimeCoroutine = StartCoroutine(LifetimeCoroutine());
        }
    }

    /// <summary>
    /// Configura el proyectil para que se dirija al jugador con un pequeño delay,
    /// haciéndolo desafiante pero esquivable si el jugador se mueve a tiempo.
    /// </summary>
    public void SetDelayedHoming(Transform target, float trackingDelay = 0.16f, float turnSpeed = 95f, float maxDuration = 1.4f, float disengageDistance = 4.5f)
    {
        if (target == null)
        {
            homingEnabled = false;
            return;
        }
        homingTarget = target;
        homingTrackingDelay = Mathf.Max(0.02f, trackingDelay);
        homingTurnSpeed = turnSpeed;
        homingMaxDuration = maxDuration;
        homingDisengageDistance = disengageDistance;
        homingElapsed = 0f;
        homingDisengaged = false;
        homingEnabled = true;

        targetHistory.Clear();
        currentDelayedTargetPos = target.position;
        targetHistory.Enqueue(new TargetSample { time = Time.time, position = target.position });
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
            lifetimeCoroutine = null;
        }
        if (Application.isPlaying && gameObject.activeInHierarchy)
        {
            lifetimeCoroutine = StartCoroutine(LifetimeCoroutine());
        }
    }

    private void OnDisable()
    {
        homingEnabled = false;
        homingTarget = null;
        targetHistory.Clear();
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
            if (homingEnabled && !homingDisengaged && homingTarget != null)
            {
                UpdateDelayedHoming();
            }

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

    private void UpdateDelayedHoming()
    {
        homingElapsed += Time.deltaTime;
        if (homingElapsed > homingMaxDuration)
        {
            homingDisengaged = true;
            return;
        }

        float now = Time.time;
        targetHistory.Enqueue(new TargetSample { time = now, position = homingTarget.position });

        // Obtener la posición del jugador hace 'homingTrackingDelay' segundos
        while (targetHistory.Count > 1 && (now - targetHistory.Peek().time) >= homingTrackingDelay)
        {
            currentDelayedTargetPos = targetHistory.Dequeue().position;
        }

        Vector3 toRealTarget = homingTarget.position - transform.position;
        toRealTarget.y = 0f;
        // Cuando la bala ya está muy cerca del jugador, deja de corregir el giro para permitir el esquive de último momento
        if (toRealTarget.magnitude <= homingDisengageDistance)
        {
            homingDisengaged = true;
            return;
        }

        Vector3 desiredDir = currentDelayedTargetPos - transform.position;
        desiredDir.y = 0f;
        if (desiredDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(desiredDir.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, homingTurnSpeed * Time.deltaTime);
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
        if (bulletData.ownerTag == "Enemy" || bulletData.ownerTag == "Boss") return "Player";
        if (bulletData.ownerTag == "Player") return "Enemy";
        return string.IsNullOrEmpty(playerTag) ? "Player" : playerTag;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (bulletData == null) return;
        if (!IsValidTarget(other)) return;
        ApplyDamageTo(other);
    }

    public void SimulateHit(Collider other)
    {
        OnTriggerEnter(other);
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
        if (bulletData.ownerTag == "Enemy" || bulletData.ownerTag == "Boss")
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
            if (bulletData.ownerTag == "Enemy" || bulletData.ownerTag == "Boss")
            {
                var playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth == null) playerHealth = other.GetComponentInParent<PlayerHealth>();
                if (playerHealth != null) playerHealth.TakeDamage(bulletData.damage);
            }
            else if (bulletData.ownerTag == "Player")
            {
                var enemyLife = other.GetComponent<EnemyLife>();
                if (enemyLife == null) enemyLife = other.GetComponentInParent<EnemyLife>();
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
