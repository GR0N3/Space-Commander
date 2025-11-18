using UnityEngine;

[RequireComponent(typeof(Collider))]
[System.Obsolete("PowerUpPickup está obsoleto. Usa PowerUp.cs con PowerUpType (patrón Type Object) en su lugar.")]
public class PowerUpPickup : MonoBehaviour
{
    [Header("Type Object - Power-Up Data")]
    public PowerUpType powerUpType;

    [Header("Configuración Legacy")]
    public PickupPowerUpType legacyPowerUpType = PickupPowerUpType.SideCannons;

    public ObjectPool dronePool;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var mgr = other.GetComponent<PlayerPowerUpManager>();
        if (mgr == null)
        {
            return;
        }

        if (powerUpType != null)
        {
            mgr.ApplyPowerUpType(powerUpType);
        }
        else
        {
            PowerUpType tempPowerUp = ScriptableObject.CreateInstance<PowerUpType>();
            tempPowerUp.powerUpName = $"Power-Up Legacy ({legacyPowerUpType})";
            switch (legacyPowerUpType)
            {
                case PickupPowerUpType.SideCannons:
                    tempPowerUp.enableSideCannons = true;
                    break;
                case PickupPowerUpType.DroneSupport:
                    tempPowerUp.enableTurrets = true;
                    break;
                case PickupPowerUpType.OrbitingDrones:
                    tempPowerUp.enableOrbitingDrones = true;
                    break;
            }
            mgr.ApplyPowerUpType(tempPowerUp);
            Destroy(tempPowerUp);
        }
        Destroy(gameObject);
    }
}

public enum PickupPowerUpType
{
    SideCannons,
    DroneSupport,
    OrbitingDrones
}
