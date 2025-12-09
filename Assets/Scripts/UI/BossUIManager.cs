using UnityEngine;
using UnityEngine.UI;

public class BossUIManager : MonoBehaviour
{
    [SerializeField] private EntityLife bossLife;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient fillGradient;
    [SerializeField] private bool hideWhenDead = true;

    private int lastMax;
    private int lastCurrent;

    void Start()
    {
        if (bossLife == null)
        {
            var boss = FindFirstObjectByType<Boss>();
            if (boss != null)
            {
                bossLife = boss.GetComponent<EntityLife>();
                if (bossLife == null)
                {
                    bossLife = boss.GetComponentInParent<EntityLife>();
                }
            }
        }
        InitializeUI();
    }

    void Update()
    {
        if (bossLife == null) return;
        int max = bossLife.MaxHealth;
        int current = bossLife.CurrentHealth;
        if (max != lastMax)
        {
            InitializeUI();
        }
        UpdateBar(current, max);
        lastCurrent = current;
    }

    void InitializeUI()
    {
        if (bossLife == null) return;
        int max = bossLife.MaxHealth;
        int current = bossLife.CurrentHealth;
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
        UpdateFillColor(current, max);
        lastMax = max;
        lastCurrent = current;
    }

    void UpdateBar(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.value = current;
        }
        UpdateFillColor(current, max);
        if (hideWhenDead && current <= 0)
        {
            if (healthSlider != null) healthSlider.gameObject.SetActive(false);
            if (fillImage != null) fillImage.gameObject.SetActive(false);
        }
    }

    void UpdateFillColor(int current, int max)
    {
        if (fillImage == null) return;
        float t = max > 0 ? (float)current / (float)max : 0f;
        if (fillGradient != null)
        {
            fillImage.color = fillGradient.Evaluate(t);
        }
        else
        {
            fillImage.fillAmount = t;
        }
    }

    public void SetBoss(EntityLife life)
    {
        bossLife = life;
        InitializeUI();
    }
}
