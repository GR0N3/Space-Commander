using UnityEngine;

public class PickupFacade : MonoBehaviour
{
    public enum PickupType { Weapon_Canons, Weapon_Turrets, Weapon_OrbitDrones, Shield, Other }

    [Header("References")]
    public AudioSource audioSource;
    public AudioClip pickupSound;
    public Animator pickupAnimator;
    public MonoBehaviour hudTarget;
    public PlayerFacade playerFacade;
    [Header("Type Object - PowerUp (Preferido)")]
    public PowerUpType powerUpType;

    public void OnPickupCollected(PickupType type, GameObject pickup, ShieldData shieldData = null)
    {
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
        if (pickupAnimator != null)
        {
            pickupAnimator.SetTrigger("Collect");
        }
        if (playerFacade != null)
        {
            if (powerUpType != null)
            {
                playerFacade.CollectPickup(powerUpType);
            }
            else
            {
                PowerUpType temp = ScriptableObject.CreateInstance<PowerUpType>();
                switch (type)
                {
                    case PickupType.Weapon_Canons:
                        temp.powerUpName = "Cañones Laterales (Legacy)";
                        temp.enableSideCannons = true;
                        break;
                    case PickupType.Weapon_Turrets:
                        temp.powerUpName = "Torretas (Legacy)";
                        temp.enableTurrets = true;
                        break;
                    case PickupType.Weapon_OrbitDrones:
                        temp.powerUpName = "Drones Orbitantes (Legacy)";
                        temp.enableOrbitingDrones = true;
                        break;
                    case PickupType.Shield:
                        temp.powerUpName = "Escudo (Legacy)";
                        temp.shieldActive = true;
                        temp.shieldResistance = 100;
                        temp.shieldLevel = 1;
                        break;
                    default:
                        temp.powerUpName = "Pickup (Legacy)";
                        break;
                }
                playerFacade.CollectPickup(temp);
                Destroy(temp);
            }
        }
        if (hudTarget != null)
        {
            hudTarget.SendMessage("OnPickupCollected", type, SendMessageOptions.DontRequireReceiver);
        }

        if (pickup != null)
        {
            pickup.SetActive(false);
        }
    }
}
