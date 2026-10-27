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
    public GameObject Drone_3;
    public GameObject Drone_4;

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

    [Header("Progresión Ordenada de Armas (Type Object)")]
    [Tooltip("Orden de mejora de armas: 0=CadenceLvl1, 1=DronesLvl1, 2=DronesLvl2, 3=DronesLvl3, 4=CadenceLvl2")]
    public PowerUpType[] weaponProgressionOrder;

    [Header("Progresión Ordenada de Escudos (Type Object)")]
    [Tooltip("Orden de mejora de escudos: 0=Shields (Nivel 1), 1=ShieldLvl2 (Nivel 2)")]
    public PowerUpType[] shieldProgressionOrder;

    private int weaponLevel = 0;
    private int shieldLevel = 0;
    private float currentFireRateMultiplier = 1f;
    public int WeaponLevel => weaponLevel;
    public int ShieldLevel => shieldLevel;
    private PlayerHealth playerHealth;
    
    public PowerUpType CurrentPowerUpType => currentPowerUpType;
    
    public float GetFireRateMultiplier()
    {
        return currentFireRateMultiplier;
    }

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null) playerHealth = GetComponentInParent<PlayerHealth>();
        EnsureDroneReferences();
        InitializeShieldComponents();
        ResetAllPowerUps();
    }

    public void EnsureDroneReferences()
    {
        Transform powerUpsRoot = transform.Find("PowerUps");
        if (Drone_L == null)
        {
            if (Turret_L != null) Drone_L = Turret_L;
            else if (powerUpsRoot != null)
            {
                var t = powerUpsRoot.Find("Turret_L");
                if (t != null) Drone_L = t.gameObject;
            }
        }
        if (Drone_R == null)
        {
            if (Turret_R != null) Drone_R = Turret_R;
            else if (powerUpsRoot != null)
            {
                var t = powerUpsRoot.Find("Turret_R");
                if (t != null) Drone_R = t.gameObject;
            }
        }
        if (Drone_3 == null && powerUpsRoot != null)
        {
            var t = powerUpsRoot.Find("Turret_3");
            if (t != null) Drone_3 = t.gameObject;
        }
        if (Drone_4 == null && powerUpsRoot != null)
        {
            var t = powerUpsRoot.Find("Turret_4");
            if (t != null) Drone_4 = t.gameObject;
        }

        // Asegurar que todos los drones tengan las referencias necesarias para disparar (BulletPool, FirePoint, etc.)
        BulletShoot mainShoot = GetComponentInChildren<BulletShoot>();
        GameObject[] allDrones = new GameObject[] { Drone_L, Drone_R, Drone_3, Drone_4 };
        foreach (var d in allDrones)
        {
            if (d == null) continue;
            var bs = d.GetComponent<BulletShoot>();
            if (bs != null)
            {
                bs.SyncPoolFrom(mainShoot, this);
            }
        }
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

        EnsureDroneReferences();

        bool isShieldPowerUp = powerUpType.shieldActive;
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

        bool isWeaponPowerUp = !isShieldPowerUp && powerUpType.healAmount <= 0 && (
                               powerUpType.weaponUpgradeMode != WeaponUpgradeMode.DefaultLegacy ||
                               powerUpType.enableSideCannons ||
                               powerUpType.enableTurrets ||
                               powerUpType.enableOrbitingDrones ||
                               powerUpType.fireRateMultiplier > 1f);

        if (isWeaponPowerUp)
        {
            // Progresión estricta en orden (sin degradaciones):
            // 1 = CadenceLvl1, 2 = DronesLvl1 (fijos), 3 = DronesLvl2 (2 orbitando), 4 = DronesLvl3 (4 orbitando), 5 = CadenceLvl2 (4 orbitando + mayor cadencia)
            IncreaseWeaponLevel(powerUpType);
        }
    }

    private void IncreaseWeaponLevel(PowerUpType pickedUpType = null)
    {
        int maxWeaponLevel = (weaponProgressionOrder != null && weaponProgressionOrder.Length > 0) ? weaponProgressionOrder.Length : 5;
        if (weaponLevel < maxWeaponLevel)
        {
            weaponLevel++;
        }

        PowerUpType stageType = GetWeaponStageType(weaponLevel, pickedUpType);
        if (stageType != null)
        {
            currentPowerUpType = stageType;
            if (stageType.fireRateMultiplier > currentFireRateMultiplier)
            {
                currentFireRateMultiplier = stageType.fireRateMultiplier;
            }
        }
        else if (pickedUpType != null)
        {
            currentPowerUpType = pickedUpType;
            if (pickedUpType.fireRateMultiplier > currentFireRateMultiplier)
            {
                currentFireRateMultiplier = pickedUpType.fireRateMultiplier;
            }
        }

        if (weaponLevel >= 1 && currentFireRateMultiplier <= 1f)
        {
            currentFireRateMultiplier = 1.35f;
        }
        if (weaponLevel >= 5 && currentFireRateMultiplier < 2.0f)
        {
            currentFireRateMultiplier = 2.0f;
        }

        switch (weaponLevel)
        {
            case 1:
                // Nivel 1: Cadence LVL1 (solo aumenta cadencia, sin drones)
                DisableAllDrones();
                break;
            case 2:
                // Nivel 2: Drones Lvl1 (2 drones fijos a los costados)
                DisableAllDrones();
                EnableDronesFixed();
                break;
            case 3:
                // Nivel 3: Drones Lvl2 (2 drones orbitando)
                DisableAllDrones();
                EnableDronesOrbit(2, stageType);
                break;
            case 4:
                // Nivel 4: Drones Lvl3 (4 drones orbitando)
                DisableAllDrones();
                EnableDronesOrbit(4, stageType);
                break;
            default:
                // Nivel 5+: Cadence LVL2 (mantiene los 4 drones orbitando y aumenta aún más la cadencia)
                if (!AreAllFourDronesOrbiting())
                {
                    DisableAllDrones();
                    EnableDronesOrbit(4, stageType);
                }
                break;
        }
    }

    private bool AreAllFourDronesOrbiting()
    {
        GameObject[] fourDrones = new GameObject[] { Drone_L, Drone_R, Drone_3, Drone_4 };
        foreach (var d in fourDrones)
        {
            if (d == null || !d.activeSelf) return false;
            var orbit = d.GetComponent<DroneOrbit>();
            if (orbit == null || !orbit.enabled) return false;
        }
        return true;
    }

    public PowerUpType GetWeaponStageType(int level, PowerUpType fallback = null)
    {
        int index = level - 1;
        if (weaponProgressionOrder != null && index >= 0 && index < weaponProgressionOrder.Length && weaponProgressionOrder[index] != null)
        {
            return weaponProgressionOrder[index];
        }
        return fallback != null ? fallback : currentPowerUpType;
    }

    public PowerUpType GetNextWeaponPowerUpType()
    {
        int maxWeaponLevel = (weaponProgressionOrder != null && weaponProgressionOrder.Length > 0) ? weaponProgressionOrder.Length : 5;
        int nextLevel = Mathf.Clamp(weaponLevel + 1, 1, maxWeaponLevel);
        return GetWeaponStageType(nextLevel, null);
    }

    public PowerUpType GetShieldStageType(int level, PowerUpType fallback = null)
    {
        int index = level - 1;
        if (shieldProgressionOrder != null && index >= 0 && index < shieldProgressionOrder.Length && shieldProgressionOrder[index] != null)
        {
            return shieldProgressionOrder[index];
        }
        return fallback;
    }

    public PowerUpType GetNextShieldPowerUpType()
    {
        SyncActiveShieldState();
        int maxShieldLevel = (shieldProgressionOrder != null && shieldProgressionOrder.Length > 0) ? shieldProgressionOrder.Length : 2;
        int nextLevel = Mathf.Clamp(shieldLevel + 1, 1, maxShieldLevel);
        return GetShieldStageType(nextLevel, null);
    }

    private void SyncActiveShieldState()
    {
        bool s1Active = shield1Component != null && shield1Component.IsActive;
        bool s2Active = shield2Component != null && shield2Component.IsActive;
        if (!s1Active && !s2Active && shieldLevel > 0)
        {
            shieldLevel = 0;
        }
    }

    private void IncreaseShieldLevel(PowerUpType powerUpType)
    {
        SyncActiveShieldState();
        // Progresión estricta en orden de escudo (Nivel 1 -> Nivel 2, sin degradaciones)
        if (shieldLevel < 2)
        {
            shieldLevel++;
        }
        shieldLevel = Mathf.Clamp(shieldLevel, 1, 2);

        PowerUpType stageShield = GetShieldStageType(shieldLevel, powerUpType);
        int resistance = GetShieldResistance(shieldLevel, stageShield);
        ApplyShield(shieldLevel, resistance);
    }

    private int GetShieldResistance(int level, PowerUpType powerUpType)
    {
        if (level == 2)
        {
            if (powerUpType != null && powerUpType.shieldLevel == 2 && powerUpType.shieldResistance > 0)
            {
                return powerUpType.shieldResistance;
            }
            return 200;
        }
        if (powerUpType != null && powerUpType.shieldResistance > 0)
        {
            return powerUpType.shieldResistance;
        }
        return 100;
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
        EnsureDroneReferences();

        if (Drone_L) 
        {
            DroneOrbit orbitL = Drone_L.GetComponent<DroneOrbit>();
            if (orbitL != null) orbitL.enabled = false;
            Drone_L.transform.SetParent(transform);
            Drone_L.transform.localPosition = new Vector3(-2.2f, 0f, 0f);
            Drone_L.transform.localRotation = Quaternion.identity;
            Drone_L.SetActive(true);
        }
        
        if (Drone_R) 
        {
            DroneOrbit orbitR = Drone_R.GetComponent<DroneOrbit>();
            if (orbitR != null) orbitR.enabled = false;
            Drone_R.transform.SetParent(transform);
            Drone_R.transform.localPosition = new Vector3(2.2f, 0f, 0f);
            Drone_R.transform.localRotation = Quaternion.identity;
            Drone_R.SetActive(true);
        }

        if (Drone_3) Drone_3.SetActive(false);
        if (Drone_4) Drone_4.SetActive(false);
    }

    private void EnableDronesOrbit(int count, PowerUpType powerUpType = null)
    {
        EnsureDroneReferences();

        float radius = powerUpType != null && powerUpType.orbitRadius > 0 ? powerUpType.orbitRadius : defaultOrbitRadius;
        float speed = powerUpType != null && powerUpType.orbitSpeed > 0 ? powerUpType.orbitSpeed : defaultOrbitSpeed;
        float baseAngle = Random.Range(0f, 360f);

        GameObject[] dronesToSetup;
        float[] angleOffsets;

        if (count == 4)
        {
            dronesToSetup = new GameObject[] { Drone_L, Drone_R, Drone_3, Drone_4 };
            angleOffsets = new float[] { 0f, 90f, 180f, 270f };
        }
        else
        {
            dronesToSetup = new GameObject[] { Drone_L, Drone_R };
            angleOffsets = new float[] { 0f, 180f };
            if (Drone_3) Drone_3.SetActive(false);
            if (Drone_4) Drone_4.SetActive(false);
        }

        for (int i = 0; i < dronesToSetup.Length; i++)
        {
            GameObject droneObj = dronesToSetup[i];
            if (droneObj == null) continue;

            droneObj.SetActive(true);
            droneObj.transform.SetParent(null);

            DroneOrbit orbit = droneObj.GetComponent<DroneOrbit>();
            if (orbit == null) orbit = droneObj.AddComponent<DroneOrbit>();

            float currentAngle = (baseAngle + angleOffsets[i]) % 360f;
            orbit.target = transform;
            orbit.radius = Mathf.Abs(radius);
            orbit.speed = speed;
            orbit.useRandomStart = false;
            orbit.angle = currentAngle;
            orbit.faceOutward = true;

            float rad = currentAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
            droneObj.transform.position = transform.position + offset;
            droneObj.transform.forward = (droneObj.transform.position - transform.position).normalized;

            orbit.enabled = true;
        }
    }

    private void DisableAllDrones()
    {
        EnsureDroneReferences();
        Transform powerUpsRoot = transform.Find("PowerUps");
        GameObject[] allDrones = new GameObject[] { Drone_L, Drone_R, Drone_3, Drone_4, Turret_L, Turret_R };
        foreach (var d in allDrones)
        {
            if (d == null) continue;
            var orbit = d.GetComponent<DroneOrbit>();
            if (orbit != null) orbit.enabled = false;
            if (powerUpsRoot != null && d.transform.parent != powerUpsRoot)
            {
                d.transform.SetParent(powerUpsRoot);
                d.transform.localPosition = Vector3.zero;
            }
            d.SetActive(false);
        }
    }

    private void DisableAllWeapons()
    {
        if (Canon_L) Canon_L.SetActive(false);
        if (Canon_R) Canon_R.SetActive(false);
        DisableAllDrones();
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
        SyncActiveShieldState();
    }

    public void OnShieldBroken()
    {
        shieldLevel = 0;
    }

    private void ResetAllPowerUps()
    {
        DisableAllWeapons();
        DisableShield();
        currentPowerUpType = null;
        weaponLevel = 0;
        shieldLevel = 0;
        currentFireRateMultiplier = 1f;
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
