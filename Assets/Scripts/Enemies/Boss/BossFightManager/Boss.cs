using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [SerializeField] public int health = 100;
    [SerializeField] private Transform player;
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float gunAttackMinDistance = 6f;
    [SerializeField] private float stopDistance = 3f;
    [SerializeField] private float cooldownBetweenAttacks = 2f;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform cannonPivot;
    [SerializeField] private Transform swordPoint;
    [SerializeField] private int swordDamage = 20;
    [SerializeField] private float swordHitRadius = 1.5f;
    [SerializeField] private float swordHitDistance = 1.5f;
    [SerializeField] private BossBulletPool bossBulletPool;
    [SerializeField] private BulletData enemyBulletData;
    [SerializeField] private EventQueue eventQueue;
    [SerializeField] private bool lockY = true;
    [SerializeField] private bool freezeTilt = true;
    [SerializeField] private float facingOffsetDegrees = 0f;
    [SerializeField] private string swordTriggerName = "SwordAttack";
    [SerializeField] private string gunTriggerName = "GunAttack";
    [SerializeField] private string isWalkingParamName = "IsWalking";
    [SerializeField] private string isDeadParamName = "IsDead";
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float visualFacingCorrectionDegrees = 0f;
    [SerializeField] private bool autoVisualFacingCorrection = true;
    [SerializeField] private LayerMask aimMask;
    [SerializeField] private float maxAimDistance = 100f;
    [SerializeField] private bool clampAimToYaw = false;
    [SerializeField] private float aimTurnSpeed = 12f;
    [SerializeField] private string swordStateName = "SwordAttack";
    [SerializeField] private string gunStateName = "GunAttack";
    [SerializeField] private float attackStateCheckDelay = 0.05f;
    [SerializeField] private float crossFadeDuration = 0.1f;

    
    private Rigidbody rb;
    private float fixedY;
    [SerializeField] private float animEventMinInterval = 0.2f;
    private float lastSwordEventTime = -999f;
    private float lastGunEventTime = -999f;

    void Awake()
    {
        if (eventQueue == null) eventQueue = GetComponent<EventQueue>();
        if (bossBulletPool == null) bossBulletPool = BossBulletPool.Instance;
        if (animator == null) animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        fixedY = transform.position.y;
        if (rb != null)
        {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | (lockY ? RigidbodyConstraints.FreezePositionY : RigidbodyConstraints.None);
        }
        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
        if (visualRoot != null)
        {
            if (Mathf.Abs(visualFacingCorrectionDegrees) > 0.01f)
            {
                visualRoot.localRotation = Quaternion.Euler(0f, visualFacingCorrectionDegrees, 0f);
            }
            else if (autoVisualFacingCorrection)
            {
                AutoCorrectVisualFacing();
            }
        }
        StateMachine = new BossStateMachine();
        StateMachine.ChangeState(new IdleState(), this);
    }

    void AutoCorrectVisualFacing()
    {
        var rootF = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        var visF = new Vector3(visualRoot.forward.x, 0f, visualRoot.forward.z).normalized;
        if (rootF.sqrMagnitude < 0.0001f || visF.sqrMagnitude < 0.0001f) return;
        var angle = Vector3.SignedAngle(visF, rootF, Vector3.up);
        visualRoot.localRotation *= Quaternion.Euler(0f, angle, 0f);
    }

    void Update()
    {
        FollowPlayer();
        StateMachine.Update(this);
        if (lockY)
        {
            var p = transform.position;
            transform.position = new Vector3(p.x, fixedY, p.z);
        }
        if (freezeTilt)
        {
            var e = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, e.y, 0f);
        }
    }

    public void FollowPlayer()
    {
        if (player == null) return;
        var targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        var toTarget = targetPos - transform.position;
        var step = followSpeed * Time.deltaTime;
        var flatDir = new Vector3(toTarget.x, 0f, toTarget.z);
        var dist = flatDir.magnitude;
        var desiredPos = dist > stopDistance ? targetPos - flatDir.normalized * stopDistance : transform.position;
        transform.position = Vector3.MoveTowards(transform.position, desiredPos, step);
        bool isWalking = Vector3.Distance(transform.position, desiredPos) > 0.01f;
        if (animator != null && !string.IsNullOrEmpty(isWalkingParamName)) animator.SetBool(isWalkingParamName, isWalking);
        if (flatDir.sqrMagnitude > 0.001f)
        {
            var lookRotation = Quaternion.LookRotation(flatDir.normalized, Vector3.up);
            if (Mathf.Abs(facingOffsetDegrees) > 0.01f)
            {
                lookRotation *= Quaternion.Euler(0f, facingOffsetDegrees, 0f);
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
        }
    }

    

    public void SwordAttack()
    {
        if (animator != null)
        {
            if (!string.IsNullOrEmpty(isWalkingParamName)) animator.SetBool(isWalkingParamName, false);
            SetTriggerSafe(swordTriggerName, "SwordAtt");
            StartCoroutine(EnsureEnterStateAfterDelay(swordStateName));
            StartCoroutine(FallbackSwordHitIfNoEvent());
        }
    }

    public void GunAttack()
    {
        if (animator != null)
        {
            if (!string.IsNullOrEmpty(isWalkingParamName)) animator.SetBool(isWalkingParamName, false);
            SetTriggerSafe(gunTriggerName, "GunAttac");
            StartCoroutine(EnsureEnterStateAfterDelay(gunStateName));
            StartCoroutine(FallbackGunShootIfNoEvent());
        }
    }

    public void ApplySwordDamage()
    {
        Vector3 center = swordPoint != null ? swordPoint.position : transform.position + transform.forward * swordHitDistance;
        var hits = Physics.OverlapSphere(center, swordHitRadius);
        foreach (var h in hits)
        {
            if (!h.CompareTag("Player")) continue;
            var entityLife = h.GetComponent<EntityLife>();
            if (entityLife != null)
            {
                entityLife.TakeDamage(swordDamage);
                continue;
            }
            var playerHealth = h.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(swordDamage);
            }
        }
    }

    public void AnimEvent_SwordHit()
    {
        if (Time.time - lastSwordEventTime < animEventMinInterval) return;
        lastSwordEventTime = Time.time;
        ApplySwordDamage();
    }

    public void AnimEvent_GunShoot()
    {
        if (Time.time - lastGunEventTime < animEventMinInterval) return;
        lastGunEventTime = Time.time;
        AimCannonAtPlayer();
        if (bossBulletPool == null || firePoint == null || enemyBulletData == null) return;
        var obj = bossBulletPool.GetBullet(firePoint.position, firePoint.rotation);
        var bullet = obj != null ? obj.GetComponent<Bullet>() : null;
        if (bullet != null) bullet.Initialize(enemyBulletData);
    }

    public void SpawnBulletAtCannon()
    {
        AimCannonAtPlayer();
        if (bossBulletPool == null || firePoint == null || enemyBulletData == null) return;
        var obj = bossBulletPool.GetBullet(firePoint.position, firePoint.rotation);
        var bullet = obj != null ? obj.GetComponent<Bullet>() : null;
        if (bullet != null) bullet.Initialize(enemyBulletData);
    }

    IEnumerator FallbackGunShootIfNoEvent()
    {
        float prev = lastGunEventTime;
        yield return new WaitForSeconds(attackStateCheckDelay + 0.25f);
        if (Mathf.Approximately(prev, lastGunEventTime))
        {
            SpawnBulletAtCannon();
        }
    }

    IEnumerator FallbackSwordHitIfNoEvent()
    {
        float prev = lastSwordEventTime;
        yield return new WaitForSeconds(attackStateCheckDelay + 0.15f);
        if (Mathf.Approximately(prev, lastSwordEventTime))
        {
            ApplySwordDamage();
        }
    }

    public void AimCannonAtPlayer()
    {
        var pivot = cannonPivot != null ? cannonPivot : firePoint;
        if (pivot == null || player == null) return;
        var toPlayer = player.position - pivot.position;
        var dir = toPlayer.normalized;
        RaycastHit hit;
        if (Physics.Raycast(pivot.position, dir, out hit, maxAimDistance, aimMask))
        {
            var aimDir = (hit.point - pivot.position).normalized;
            if (clampAimToYaw)
            {
                aimDir = new Vector3(aimDir.x, 0f, aimDir.z).normalized;
                if (aimDir.sqrMagnitude < 0.0001f) return;
                var yaw = Quaternion.LookRotation(aimDir, Vector3.up);
                pivot.rotation = Quaternion.Slerp(pivot.rotation, yaw, aimTurnSpeed * Time.deltaTime);
            }
            else
            {
                var rot = Quaternion.LookRotation(aimDir, Vector3.up);
                pivot.rotation = Quaternion.Slerp(pivot.rotation, rot, aimTurnSpeed * Time.deltaTime);
            }
        }
        else
        {
            var aimDir = dir;
            if (clampAimToYaw)
            {
                aimDir = new Vector3(aimDir.x, 0f, aimDir.z).normalized;
                if (aimDir.sqrMagnitude < 0.0001f) return;
                var yaw = Quaternion.LookRotation(aimDir, Vector3.up);
                pivot.rotation = Quaternion.Slerp(pivot.rotation, yaw, aimTurnSpeed * Time.deltaTime);
            }
            else
            {
                var rot = Quaternion.LookRotation(aimDir, Vector3.up);
                pivot.rotation = Quaternion.Slerp(pivot.rotation, rot, aimTurnSpeed * Time.deltaTime);
            }
        }
    }

    void SetTriggerSafe(string primary, string fallback)
    {
        if (!string.IsNullOrEmpty(primary)) animator.SetTrigger(primary);
        if (!string.IsNullOrEmpty(fallback) && fallback != primary) animator.SetTrigger(fallback);
    }

    System.Collections.IEnumerator EnsureEnterStateAfterDelay(string stateName)
    {
        yield return new WaitForSeconds(attackStateCheckDelay);
        var info = animator.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName(stateName))
        {
            animator.CrossFade(stateName, crossFadeDuration, 0);
        }
    }

    public void Die()
    {
        health = 0;
        gameObject.SetActive(false);
        if (animator != null && !string.IsNullOrEmpty(isDeadParamName)) animator.SetBool(isDeadParamName, true);
    }

    public BossMemento SaveState()
    {
        return new BossMemento(transform.position, health);
    }

    public void RestoreState(BossMemento memento)
    {
        if (memento == null) return;
        transform.position = memento.position;
        health = memento.health;
    }

    public BossStateMachine StateMachine { get; private set; }
    public Transform Player => player;
    public float AttackRange => attackRange;
    public float StopDistance => stopDistance;
    public float GunAttackMinDistance => gunAttackMinDistance;
    public float CooldownBetweenAttacks => cooldownBetweenAttacks;
    public EventQueue EventQueue => eventQueue;
    public Animator Animator => animator;
    public string IsWalkingParamName => isWalkingParamName;
    public int SwordDamage => swordDamage;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, gunAttackMinDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}

