using UnityEngine;

public class BulletShoot : MonoBehaviour, IWeapon
{
    [Header("Pooling y Posición")]
    [Tooltip("Asigna tu pool de balas. Puede ser un ObjectPool (pool por jugador) o un BulletPool (pool global). El script usará lo que asignes.")]
    [SerializeField] private ObjectPool objectPool;
    [Tooltip("Si tienes un BulletPool singleton o específico, asígnalo aquí (opcional)")]
    [SerializeField] private BulletPool bulletPool;
    [SerializeField] private Transform firePoint;

    [Header("Tipo de Proyectil (Flyweight Pattern)")]
    [Tooltip("ScriptableObject que define las propiedades de la bala a disparar (Player o Enemigo)")]
    [SerializeField] private BulletData bulletData;

    [Header("Velocidad de Disparo")]
    [Tooltip("Tiempo entre disparos en segundos (cooldown base)")]
    [SerializeField] private float baseFireRate = 0.5f;
    
    [Tooltip("Referencia al PlayerPowerUpManager para obtener el multiplicador de velocidad de disparo")]
    [SerializeField] private PlayerPowerUpManager powerUpManager;

    private float fireCooldown = 0f;

    void Start()
    {
        EnsureReferences();
    }

    public void SyncPoolFrom(BulletShoot source, PlayerPowerUpManager mgr)
    {
        if (mgr != null && powerUpManager == null)
        {
            powerUpManager = mgr;
        }
        if (source != null && source != this)
        {
            if (bulletPool == null && source.bulletPool != null) bulletPool = source.bulletPool;
            if (objectPool == null && source.objectPool != null) objectPool = source.objectPool;
            if (bulletData == null && source.bulletData != null) bulletData = source.bulletData;
        }
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        if (powerUpManager == null)
        {
            powerUpManager = GetComponent<PlayerPowerUpManager>();
            if (powerUpManager == null)
            {
                powerUpManager = GetComponentInParent<PlayerPowerUpManager>();
            }
            if (powerUpManager == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) powerUpManager = p.GetComponent<PlayerPowerUpManager>();
            }
        }

        if (firePoint == null)
        {
            var canon = transform.Find("Canon");
            if (canon != null) firePoint = canon;
            else firePoint = transform;
        }

        if (objectPool == null && bulletPool == null)
        {
            // Intentar heredar el pool del cañón principal del jugador o buscar en la escena
            if (powerUpManager != null)
            {
                var allShoots = powerUpManager.GetComponentsInChildren<BulletShoot>(true);
                foreach (var s in allShoots)
                {
                    if (s != this && (s.bulletPool != null || s.objectPool != null))
                    {
                        this.bulletPool = s.bulletPool;
                        this.objectPool = s.objectPool;
                        break;
                    }
                }
            }
            if (objectPool == null && bulletPool == null)
            {
                var bpObj = GameObject.Find("PlayerBulletPool");
                if (bpObj != null) bulletPool = bpObj.GetComponent<BulletPool>();
                if (bulletPool == null) bulletPool = FindFirstObjectByType<BulletPool>();
            }
        }
    }

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        float fireRateMultiplier = 1f;
        if (powerUpManager != null)
        {
            fireRateMultiplier = powerUpManager.GetFireRateMultiplier();
        }

        float effectiveFireRate = baseFireRate / fireRateMultiplier;

        if (Input.GetKey(KeyCode.Mouse0) && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = effectiveFireRate;
        }
    }

    void OnEnable()
    {
        EnsureReferences();
    }

    public void Shoot()
    {
        if (objectPool == null && bulletPool == null)
        {
            EnsureReferences();
        }
        if (bulletData == null)
        {
            return;
        }
        if (objectPool == null && bulletPool == null)
        {
            return;
        }
        if (firePoint == null)
        {
            return;
        }

        GameObject bulletObj = null;

        if (objectPool != null)
        {
            bulletObj = objectPool.GetObject();
            if (bulletObj == null) return;
            bulletObj.transform.position = firePoint.position;
            bulletObj.transform.rotation = firePoint.rotation;
            bulletObj.SetActive(true);
        }
        else if (bulletPool != null)
        {
            bulletObj = bulletPool.GetBullet(firePoint.position, firePoint.rotation);
            if (bulletObj == null) return;
        }

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(bulletData);
        }
        else { }

        if (bulletData.shootSound != null)
        {
            var src = GetComponent<AudioSource>();
            if (src == null)
            {
                src = gameObject.AddComponent<AudioSource>();
            }
            src.spatialBlend = 0f;
            src.PlayOneShot(bulletData.shootSound, bulletData.shootVolume);
        }
    }

    public void Fire()
    {
        Shoot();
    }
}
