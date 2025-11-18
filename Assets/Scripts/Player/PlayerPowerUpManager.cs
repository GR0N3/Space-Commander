using System.Collections;
using UnityEngine;

public class PlayerPowerUpManager : MonoBehaviour
{
    [Header("Weapon Parts")]
    public GameObject Canon_L;
    public GameObject Canon_R;
    public GameObject Turret_L;
    public GameObject Turret_R;
    public GameObject Drone_L;
    public GameObject Drone_R;

    [Header("Shield Parts")]
    public GameObject Shell_1;
    public GameObject Shell_2;

    [Header("Shield Components")]
    [Tooltip("Componente de escudo para Shell_1 (se agregará automáticamente si no está asignado)")]
    public ShieldComponent shield1Component;
    
    [Tooltip("Componente de escudo para Shell_2 (se agregará automáticamente si no está asignado)")]
    public ShieldComponent shield2Component;

    [Header("Orbit Settings")]
    public float defaultOrbitRadius = 5f;
    public float defaultOrbitSpeed = 60f;

    [Header("Type Object - Power-Up Actual")]
    [SerializeField] private PowerUpType currentPowerUpType;

    private int weaponLevel = 0;
    private int shieldLevel = 0;
    public int WeaponLevel => weaponLevel;
    public int ShieldLevel => shieldLevel;
    private PlayerHealth playerHealth;
    
    public PowerUpType CurrentPowerUpType => currentPowerUpType;
    
    public float GetFireRateMultiplier()
    {
        return currentPowerUpType != null ? currentPowerUpType.fireRateMultiplier : 1f;
    }

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null) playerHealth = GetComponentInParent<PlayerHealth>();
        InitializeShieldComponents();
        ResetAllPowerUps();
    }

    private void InitializeShieldComponents()
    {
        if (Shell_1 != null && shield1Component == null)
        {
            shield1Component = Shell_1.GetComponent<ShieldComponent>();
            if (shield1Component == null)
            {
                shield1Component = Shell_1.AddComponent<ShieldComponent>();
            }
            shield1Component.shieldVisual = Shell_1;
        }
        if (Shell_2 != null && shield2Component == null)
        {
            shield2Component = Shell_2.GetComponent<ShieldComponent>();
            if (shield2Component == null)
            {
                shield2Component = Shell_2.AddComponent<ShieldComponent>();
            }
            shield2Component.shieldVisual = Shell_2;
        }
    }

    public void ApplyPowerUpType(PowerUpType powerUpType)
    {
        if (powerUpType == null)
        {
            return;
        }
        bool isWeaponPowerUp = powerUpType.enableSideCannons || powerUpType.enableTurrets || powerUpType.enableOrbitingDrones;
        bool isShieldPowerUp = powerUpType.shieldActive;
        currentPowerUpType = powerUpType;
        if (isWeaponPowerUp)
        {
            IncreaseWeaponLevel();
        }
        
        if (isShieldPowerUp)
        {
            IncreaseShieldLevel(powerUpType);
        }

        if (powerUpType.healAmount > 0)
        {
            if (playerHealth != null)
            {
                playerHealth.Heal(powerUpType.healAmount);
            }
            else
            {
                var el = GetComponent<EntityLife>();
                if (el == null) el = GetComponentInParent<EntityLife>();
                if (el != null) el.Heal(powerUpType.healAmount);
            }
        }

    }

    private void IncreaseWeaponLevel()
    {
        weaponLevel++;
        weaponLevel = Mathf.Clamp(weaponLevel, 0, 3);
        if (weaponLevel >= 1)
        {
            EnableCanons();
        }
        
        if (weaponLevel >= 2)
        {
            EnableTurrets();
        }
        
        if (weaponLevel >= 3)
        {
            EnableDronesOrbit(currentPowerUpType);
        }
    }

    private void IncreaseShieldLevel(PowerUpType powerUpType)
    {
        shieldLevel++;
        shieldLevel = Mathf.Clamp(shieldLevel, 0, 2);
        int resistance = GetShieldResistance(shieldLevel, powerUpType);
        ApplyShield(shieldLevel, resistance);
    }

    private int GetShieldResistance(int level, PowerUpType powerUpType)
    {
        if (powerUpType != null && powerUpType.shieldResistance > 0)
        {
            return powerUpType.shieldResistance;
        }
        switch (level)
        {
            case 1:
                return 100;
            case 2:
                return 200;
            default:
                return 100;
        }
    }

    [System.Obsolete("Usa ApplyPowerUpType(PowerUpType) en su lugar para usar el patrón Type Object")]
    public void ApplyWeaponPowerUp()
    {
    }

    private void EnableCanons()
    {
        if (Canon_L) Canon_L.SetActive(true);
        if (Canon_R) Canon_R.SetActive(true);
    }

    private void EnableTurrets()
    {
        if (Turret_L) Turret_L.SetActive(true);
        if (Turret_R) Turret_R.SetActive(true);
    }

    private void EnableDronesFixed()
    {
        if (Drone_L) 
        {
            Drone_L.SetActive(true);
            DroneOrbit orbitL = Drone_L.GetComponent<DroneOrbit>();
            if (orbitL != null) orbitL.enabled = false;
            if (Drone_L.transform.parent != transform)
            {
                Drone_L.transform.SetParent(transform);
            }
            Drone_L.transform.localPosition = new Vector3(-1f, 0f, 0f);
        }
        
        if (Drone_R) 
        {
            Drone_R.SetActive(true);
            DroneOrbit orbitR = Drone_R.GetComponent<DroneOrbit>();
            if (orbitR != null) orbitR.enabled = false;
            if (Drone_R.transform.parent != transform)
            {
                Drone_R.transform.SetParent(transform);
            }
            Drone_R.transform.localPosition = new Vector3(1f, 0f, 0f);
        }
    }

    private void EnableDronesOrbit(PowerUpType powerUpType = null)
    {
        if (!Drone_L || !Drone_R) return;
        float radius = powerUpType != null ? powerUpType.orbitRadius : defaultOrbitRadius;
        float speed = powerUpType != null ? powerUpType.orbitSpeed : defaultOrbitSpeed;
        Drone_L.SetActive(true);
        Drone_R.SetActive(true);
        Drone_L.transform.SetParent(null);
        Drone_R.transform.SetParent(null);
        float baseAngle = Random.Range(0f, 360f);
        float leftAngle = baseAngle + 180f;
        float rightAngle = baseAngle;
        if (Drone_L)
        {
            DroneOrbit orbit = Drone_L.GetComponent<DroneOrbit>();
            if (orbit == null) orbit = Drone_L.AddComponent<DroneOrbit>();
            
            orbit.target = transform;
            orbit.radius = Mathf.Abs(radius);
            orbit.speed = speed;
            orbit.useRandomStart = false;
            orbit.angle = leftAngle;
            orbit.faceOutward = true;
            
            float rad = leftAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
            Drone_L.transform.position = transform.position + offset;
            
            if (orbit.faceOutward)
            {
                Drone_L.transform.forward = (Drone_L.transform.position - transform.position).normalized;
            }
            
            orbit.enabled = true;
        }
        if (Drone_R)
        {
            DroneOrbit orbit = Drone_R.GetComponent<DroneOrbit>();
            if (orbit == null) orbit = Drone_R.AddComponent<DroneOrbit>();
            
            orbit.target = transform;
            orbit.radius = Mathf.Abs(radius);
            orbit.speed = speed;
            orbit.useRandomStart = false;
            orbit.angle = rightAngle;
            orbit.faceOutward = true;
            
            float rad = rightAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
            Drone_R.transform.position = transform.position + offset;
            
            if (orbit.faceOutward)
            {
                Drone_R.transform.forward = (Drone_R.transform.position - transform.position).normalized;
            }
            
            orbit.enabled = true;
        }
    }

    private void DisableAllWeapons()
    {
        if (Canon_L) Canon_L.SetActive(false);
        if (Canon_R) Canon_R.SetActive(false);
        if (Turret_L) Turret_L.SetActive(false);
        if (Turret_R) Turret_R.SetActive(false);
        if (Drone_L) 
        {
            Drone_L.SetActive(false);
            if (Drone_L.transform.parent == transform)
            {
                Drone_L.transform.SetParent(null);
            }
        }
        if (Drone_R) 
        {
            Drone_R.SetActive(false);
            if (Drone_R.transform.parent == transform)
            {
                Drone_R.transform.SetParent(null);
            }
        }
    }

    private void DisableShield()
    {
        if (shield1Component != null)
        {
            shield1Component.DeactivateShield();
        }
        if (shield2Component != null)
        {
            shield2Component.DeactivateShield();
        }

        if (Shell_1) Shell_1.SetActive(false);
        if (Shell_2) Shell_2.SetActive(false);
    }

    public void OnShieldDeactivated()
    {
        shieldLevel = Mathf.Max(0, shieldLevel - 1);
    }

    public void OnShieldBroken()
    {
        shieldLevel = Mathf.Max(0, shieldLevel - 1);
    }

    private void ResetAllPowerUps()
    {
        DisableAllWeapons();
        DisableShield();
        currentPowerUpType = null;
        weaponLevel = 0;
        shieldLevel = 0;
    }

    private void ApplyShield(int shieldLevel, int resistance)
    {
        if (shield1Component != null) shield1Component.DeactivateShield();
        if (shield2Component != null) shield2Component.DeactivateShield();

        if (shieldLevel == 1)
        {
            if (shield1Component != null)
            {
                shield1Component.ActivateShield(resistance);
            }
            else if (Shell_1 != null)
            {
                Shell_1.SetActive(true);
                ShieldComponent temp = Shell_1.GetComponent<ShieldComponent>();
                if (temp == null) temp = Shell_1.AddComponent<ShieldComponent>();
                temp.shieldVisual = Shell_1;
                temp.ActivateShield(resistance);
            }
        }
        else if (shieldLevel == 2)
        {
            if (shield2Component != null)
            {
                shield2Component.ActivateShield(resistance);
            }
            else if (Shell_2 != null)
            {
                Shell_2.SetActive(true);
                ShieldComponent temp = Shell_2.GetComponent<ShieldComponent>();
                if (temp == null) temp = Shell_2.AddComponent<ShieldComponent>();
                temp.shieldVisual = Shell_2;
                temp.ActivateShield(resistance);
            }
        }
    }

    [System.Obsolete("Usa ApplyPowerUpType(PowerUpType) en su lugar para usar el patrón Type Object")]
    public void ApplyShieldPowerUp(ShieldData data)
    {
        
        if (data == null) return;
        ApplyShield(1, 100);
    }
}
