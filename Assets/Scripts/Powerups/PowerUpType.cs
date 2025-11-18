using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerUpType", menuName = "Type Object/PowerUpType", order = 1)]
public class PowerUpType : ScriptableObject
{
    [Header("Identificación")]
    [Tooltip("Nombre descriptivo del power-up")]
    public string powerUpName = "Nuevo Power-Up";
    
    [Tooltip("Descripción del power-up (opcional, para UI)")]
    [TextArea(2, 4)]
    public string description = "";

    [Header("Armas - Cañones Extra")]
    [Tooltip("Número de cañones adicionales que se activan (0 = ninguno, 1 = cañones laterales, 2 = torretas, 3 = drones orbitantes)")]
    [Range(0, 3)]
    public int extraCannons = 0;
    
    [Tooltip("Si está activo, habilita los cañones laterales (Canon_L y Canon_R)")]
    public bool enableSideCannons = false;
    
    [Tooltip("Si está activo, habilita las torretas (Turret_L y Turret_R)")]
    public bool enableTurrets = false;
    
    [Tooltip("Si está activo, habilita los drones orbitantes (Drone_L y Drone_R)")]
    public bool enableOrbitingDrones = false;

    [Header("Velocidad de Disparo")]
    [Tooltip("Multiplicador de la velocidad de disparo (1.0 = normal, 2.0 = el doble de rápido, 0.5 = la mitad)")]
    [Range(0.1f, 5f)]
    public float fireRateMultiplier = 1f;

    [Header("Escudo")]
    [Tooltip("Si está activo, activa un escudo al recoger este power-up")]
    public bool shieldActive = false;
    
    [Tooltip("Resistencia del escudo (cuánto daño puede absorber antes de romperse). Shield 1 = 100, Shield 2 = 200")]
    [Range(0, 1000)]
    public int shieldResistance = 100;
    
    [Tooltip("Nivel del escudo (1 = Shell_1 con resistencia 100, 2 = Shell_2 con resistencia 200)")]
    [Range(1, 2)]
    public int shieldLevel = 1;

    [Header("Configuración de Drones (si enableOrbitingDrones es true)")]
    [Tooltip("Radio de órbita de los drones")]
    public float orbitRadius = 5f;
    
    [Tooltip("Velocidad de órbita en grados por segundo")]
    public float orbitSpeed = 60f;

    [Header("Curación")]
    [Range(0, 1000)]
    public int healAmount = 0;

    [Header("Efectos Visuales/Sonido (Opcional)")]
    [Tooltip("Sprite o icono del power-up (para UI)")]
    public Sprite icon;
    
    [Tooltip("Sonido que se reproduce al recoger el power-up")]
    public AudioClip pickupSound;
    public float pickupVolume = 1f;

    private void OnValidate()
    {
        if (enableOrbitingDrones)
        {
            extraCannons = 3;
        }
        else if (enableTurrets)
        {
            extraCannons = 2;
        }
        else if (enableSideCannons)
        {
            extraCannons = 1;
        }
        shieldLevel = Mathf.Clamp(shieldLevel, 1, 2);
        if (shieldActive && shieldResistance <= 0)
        {
            switch (shieldLevel)
            {
                case 1:
                    shieldResistance = 100;
                    break;
                case 2:
                    shieldResistance = 200;
                    break;
            }
        }

        healAmount = Mathf.Max(0, healAmount);
    }
}

