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
        if (powerUpManager == null)
        {
            powerUpManager = GetComponent<PlayerPowerUpManager>();
            if (powerUpManager == null)
            {
                powerUpManager = GetComponentInParent<PlayerPowerUpManager>();
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

    public void Shoot()
    {
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
