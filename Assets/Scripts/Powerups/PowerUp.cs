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

    void Start()
    {
        if (powerUpType == null && useRandomFromList && possiblePowerUpTypes != null && possiblePowerUpTypes.Length > 0)
        {
            powerUpType = possiblePowerUpTypes[Random.Range(0, possiblePowerUpTypes.Length)];
        }
        var col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
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
