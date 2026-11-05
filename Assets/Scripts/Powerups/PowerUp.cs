using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PowerUp : MonoBehaviour
{
    [Header("Type Object - Power-Up Data")]
    [Tooltip("PowerUpType que define el comportamiento de este power-up. Asigna un ScriptableObject desde el Inspector.")]
    public PowerUpType powerUpType;

    [Header("Configuración de Spawn (Opcional)")]
    [Tooltip("Si está activo y powerUpType es null, se elegirá aleatoriamente de la lista")]
    public bool useRandomFromList = false;
    
    [Tooltip("Lista de PowerUpTypes posibles si useRandomFromList está activo")]
    public PowerUpType[] possiblePowerUpTypes;

    [Header("Materiales por Defecto")]
    [Tooltip("Material para power-ups de escudo (si no se especifica en el PowerUpType)")]
    public Material shieldMaterial;
    [Tooltip("Material para power-ups de vida (si no se especifica en el PowerUpType)")]
    public Material lifeMaterial;
    [Tooltip("Material base original para power-ups de arma")]
    public Material defaultWeaponMaterial;

    void Awake()
    {
        var mr = GetComponent<MeshRenderer>();
        if (mr == null) mr = GetComponentInChildren<MeshRenderer>();
        if (mr != null && defaultWeaponMaterial == null)
        {
            defaultWeaponMaterial = mr.sharedMaterial;
        }
    }

    void Start()
    {
        if (powerUpType == null && useRandomFromList && possiblePowerUpTypes != null && possiblePowerUpTypes.Length > 0)
        {
            powerUpType = SelectAppropriatePowerUpType(possiblePowerUpTypes);
        }
        var col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        ApplyVisuals();
    }

    void OnEnable()
    {
        ApplyVisuals();
    }

    public static PowerUpType SelectAppropriatePowerUpType(PowerUpType[] candidates)
    {
        if (candidates == null || candidates.Length == 0) return null;

        PowerUpType chosen = candidates[Random.Range(0, candidates.Length)];
        if (chosen == null) return null;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        PlayerPowerUpManager mgr = playerObj != null ? playerObj.GetComponent<PlayerPowerUpManager>() : null;

        // 1. Si salió un power-up de Escudo, devolver el siguiente nivel de escudo en orden (Lvl 1 -> Lvl 2)
        if (chosen.shieldActive)
        {
            if (mgr != null)
            {
                PowerUpType nextShield = mgr.GetNextShieldPowerUpType();
                if (nextShield != null)
                {
                    return nextShield;
                }
            }
            return chosen;
        }

        // 2. Si salió un power-up de Vida, devolverlo directamente
        if (chosen.healAmount > 0)
        {
            return chosen;
        }

        // 3. Si salió un power-up de Arma, devolver el siguiente nivel de arma en orden (CadenceLvl1 -> DronesLvl1 -> DronesLvl2 -> DronesLvl3 -> CadenceLvl2)
        bool isWeapon = chosen.weaponUpgradeMode != WeaponUpgradeMode.DefaultLegacy ||
                        chosen.enableSideCannons ||
                        chosen.enableTurrets ||
                        chosen.enableOrbitingDrones ||
                        chosen.fireRateMultiplier > 1f;

        if (isWeapon && mgr != null)
        {
            PowerUpType nextWeapon = mgr.GetNextWeaponPowerUpType();
            if (nextWeapon != null)
            {
                return nextWeapon;
            }
        }
        return chosen;
    }

    public void SetPowerUpType(PowerUpType type)
    {
        powerUpType = type;
        ApplyVisuals();
    }

    public void ApplyVisuals()
    {
        if (powerUpType == null) return;
        var mr = GetComponent<MeshRenderer>();
        if (mr == null) mr = GetComponentInChildren<MeshRenderer>();
        if (mr == null) return;

        if (powerUpType.powerUpMaterial != null)
        {
            mr.sharedMaterial = powerUpType.powerUpMaterial;
        }
        else if (powerUpType.shieldActive && shieldMaterial != null)
        {
            mr.sharedMaterial = shieldMaterial;
        }
        else if (powerUpType.healAmount > 0 && lifeMaterial != null)
        {
            mr.sharedMaterial = lifeMaterial;
        }
        else if (defaultWeaponMaterial != null)
        {
            mr.sharedMaterial = defaultWeaponMaterial;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyToPlayer(other.gameObject);
            gameObject.SetActive(false);
        }
    }

    public void SimulateCollect(GameObject player)
    {
        if (player != null && player.CompareTag("Player"))
        {
            ApplyToPlayer(player);
            gameObject.SetActive(false);
        }
    }

    private void ApplyToPlayer(GameObject player)
    {
        if (powerUpType == null)
        {
            return;
        }

        PlayerPowerUpManager mgr = player.GetComponent<PlayerPowerUpManager>();
        if (mgr == null)
        {
            return;
        }

        mgr.ApplyPowerUpType(powerUpType);

        if (powerUpType.pickupSound != null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.PlayOneShot(powerUpType.pickupSound, powerUpType.pickupVolume);
        }
    }
}
