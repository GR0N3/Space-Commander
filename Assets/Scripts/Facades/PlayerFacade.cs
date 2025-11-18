using UnityEngine;

public class PlayerFacade : MonoBehaviour
{
    [Header("Player Components")]
    public BulletShoot playerShooting;
    public MonoBehaviour weapon;
    public PlayerPowerUpManager powerUpManager;
    public PlayerHealth playerHealth;

    public void Shoot()
    {
        var iw = weapon as IWeapon;
        if (iw != null)
        {
            iw.Fire();
            return;
        }
        if (playerShooting == null) return;
        playerShooting.Shoot();
    }

    public void TakeDamage(int amount)
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(amount);
        }
    }

    public void CollectPickup(PowerUpType powerUpType)
    {
        if (powerUpManager == null) return;
        if (powerUpType == null)
        {
            return;
        }
        
        powerUpManager.ApplyPowerUpType(powerUpType);
    }

    [System.Obsolete("Usa CollectPickup(PowerUpType) en su lugar para usar el patrón Type Object")]
    public void CollectPickup(PickupFacade.PickupType type, ShieldData shieldData = null)
    {
        if (powerUpManager == null) return;

        PowerUpType tempPowerUp = ScriptableObject.CreateInstance<PowerUpType>();
        
        switch (type)
        {
            case PickupFacade.PickupType.Weapon_Canons:
                tempPowerUp.powerUpName = "Cañones Laterales (Legacy)";
                tempPowerUp.enableSideCannons = true;
                break;
            case PickupFacade.PickupType.Weapon_Turrets:
                tempPowerUp.powerUpName = "Torretas (Legacy)";
                tempPowerUp.enableTurrets = true;
                break;
            case PickupFacade.PickupType.Weapon_OrbitDrones:
                tempPowerUp.powerUpName = "Drones Orbitantes (Legacy)";
                tempPowerUp.enableOrbitingDrones = true;
                break;
            case PickupFacade.PickupType.Shield:
                tempPowerUp.powerUpName = "Escudo (Legacy)";
                tempPowerUp.shieldActive = true;
                tempPowerUp.shieldResistance = 100;
                tempPowerUp.shieldLevel = 1;
                break;
            default:
                Destroy(tempPowerUp);
                return;
        }

        powerUpManager.ApplyPowerUpType(tempPowerUp);
        
        Destroy(tempPowerUp);
    }
}
