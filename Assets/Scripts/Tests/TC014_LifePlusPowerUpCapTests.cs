using NUnit.Framework;
using UnityEditor;
using UnityEngine;

[TestFixture]
[Category("PowerUps")]
[Category("AnalisisDeValoresLimite")]
public class TC014_LifePlusPowerUpCapTests
{
    private const int MaxPlayerHealth = 100;
    private const int PreconditionHealth = 80;
    private const int LifePlusHealAmount = 50;

    private GameObject playerObject;
    private PlayerFacade playerFacade;
    private PickupFacade pickupFacade;
    private EntityLife playerEntityLife;
    private PlayerHealth playerHealth;
    private PlayerPowerUpManager powerUpManager;
    private LifeType playerLifeType;
    private GameObject powerUpObject;
    private PowerUp powerUpComponent;
    private PowerUpType lifePlusType;
    private bool createdRuntimePowerUpAsset;

    [SetUp]
    public void SetUp()
    {
        playerLifeType = ScriptableObject.CreateInstance<LifeType>();
        playerLifeType.entityName = "PlayerLifeType";
        playerLifeType.maxHealth = MaxPlayerHealth;
        playerLifeType.hasShield = false;
        playerLifeType.shieldHealth = 0;

        lifePlusType = AssetDatabase.LoadAssetAtPath<PowerUpType>("Assets/Scripts/Objects/Life+.asset");
        if (lifePlusType == null)
        {
            lifePlusType = ScriptableObject.CreateInstance<PowerUpType>();
            lifePlusType.powerUpName = "+50 Life";
            lifePlusType.healAmount = LifePlusHealAmount;
            lifePlusType.shieldActive = false;
            createdRuntimePowerUpAsset = true;
        }

        playerObject = new GameObject("Player_TC014");
        playerObject.tag = "Player";
        playerEntityLife = playerObject.AddComponent<EntityLife>();
        playerEntityLife.lifeType = playerLifeType;
        playerHealth = playerObject.AddComponent<PlayerHealth>();
        playerHealth.playerLifeType = playerLifeType;
        playerHealth.InitializeHealth();
        powerUpManager = playerObject.AddComponent<PlayerPowerUpManager>();

        playerFacade = playerObject.AddComponent<PlayerFacade>();
        playerFacade.playerHealth = playerHealth;
        playerFacade.powerUpManager = powerUpManager;

        playerFacade.TakeDamage(MaxPlayerHealth - PreconditionHealth);

        powerUpObject = new GameObject("PowerUp_LifePlus_TC014");
        powerUpObject.AddComponent<SphereCollider>();
        powerUpComponent = powerUpObject.AddComponent<PowerUp>();
        powerUpComponent.powerUpType = lifePlusType;

        pickupFacade = powerUpObject.AddComponent<PickupFacade>();
        pickupFacade.playerFacade = playerFacade;
        pickupFacade.powerUpType = lifePlusType;
    }

    [TearDown]
    public void TearDown()
    {
        if (playerObject != null) Object.DestroyImmediate(playerObject);
        if (powerUpObject != null) Object.DestroyImmediate(powerUpObject);
        if (playerLifeType != null) Object.DestroyImmediate(playerLifeType);
        if (createdRuntimePowerUpAsset && lifePlusType != null) Object.DestroyImmediate(lifePlusType);
    }

    [Test]
    [Description("TC-014: Con la vida en 80, al recoger el power-up Life+ (+50), la vida queda en 100 (máximo) y no en 130.")]
    public void TC014_RecogerPowerUpLifePlus_ConVidaEn80_VidaQuedaEn100YNoEn130()
    {
        Assert.AreEqual(PreconditionHealth, playerHealth.currentHealth, "Precondición: la vida debe estar en 80.");

        pickupFacade.OnPickupCollected(PickupFacade.PickupType.Other, powerUpObject);

        Assert.AreEqual(MaxPlayerHealth, playerHealth.currentHealth,
            "La vida debe quedar limitada en 100 ([vida máxima del jugador]) y no superar el tope (130).");
        Assert.AreNotEqual(130, playerHealth.currentHealth);
        Assert.IsFalse(powerUpObject.activeSelf, "El power-up debe desactivarse tras ser recogido.");
    }

    [TestCase(49, 99, TestName = "TC014_AVL_VidaPrevia49_Mas50_QuedaEn99")]
    [TestCase(50, 100, TestName = "TC014_AVL_ValorLimiteExacto_VidaPrevia50_Mas50_QuedaEn100")]
    [TestCase(51, 100, TestName = "TC014_AVL_VecinoSuperior_VidaPrevia51_Mas50_ClampEn100")]
    [TestCase(80, 100, TestName = "TC014_AVL_CasoExcel_VidaPrevia80_Mas50_ClampEn100")]
    [TestCase(100, 100, TestName = "TC014_AVL_TopeMaximo_VidaPrevia100_Mas50_PermaneceEn100")]
    public void TC014_AnalisisValoresLimite_CuracionLifePlusEnFronteraDeVidaMaxima(int startingHealth, int expectedHealthAfterHeal)
    {
        playerHealth.InitializeHealth();
        int damageToSetup = MaxPlayerHealth - startingHealth;
        if (damageToSetup > 0)
        {
            playerFacade.TakeDamage(damageToSetup);
        }
        Assert.AreEqual(startingHealth, playerHealth.currentHealth);

        playerFacade.CollectPickup(lifePlusType);

        Assert.AreEqual(expectedHealthAfterHeal, playerHealth.currentHealth);
    }
}
