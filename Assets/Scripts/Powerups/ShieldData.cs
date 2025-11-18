using UnityEngine;

[CreateAssetMenu(fileName = "NewShieldData", menuName = "Flyweight/ShieldData", order = 1)]
public class ShieldData : ScriptableObject
{
    [Tooltip("Duración en segundos del escudo cuando se recoge")]
    public float duration = 5f;

    [Tooltip("Tipo de escudo (se puede usar para elegir visuales/comportamiento)")]
    public string shieldType = "Default";
}
