using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerPowerUpManager powerUpManager;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI weaponPowerText;
    [SerializeField] private TextMeshProUGUI shieldPowerText;

    void Start()
    {
        RefreshAll();
    }

    void Update()
    {
        RefreshAll();
    }

    private void RefreshAll()
    {
        if (playerHealth != null)
        {
            var cur = playerHealth.currentHealth;
            var max = playerHealth.maxHealth;
            int cap = 100;
            int displayMax = Mathf.Min(max, cap);
            int displayCur = Mathf.Min(cur, displayMax);
            if (healthSlider != null)
            {
                healthSlider.maxValue = displayMax;
                healthSlider.value = displayCur;
            }
            if (healthText != null) healthText.text = $"Life: {displayCur}/{displayMax}";
        }
        if (powerUpManager != null)
        {
            if (weaponPowerText != null) weaponPowerText.text = $"{powerUpManager.WeaponLevel}";
            if (shieldPowerText != null) shieldPowerText.text = $"{powerUpManager.ShieldLevel}";
        }
    }
}
