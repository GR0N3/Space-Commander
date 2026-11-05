using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("VidaDelJugador")]
[Category("ParticionDeEquivalencias")]
public class TC005_PlayerInitialStatsTests
{
    private const int ExpectedMaxHealth = 100;
    private const int ExpectedBaseShield = 100;

    private GameObject playerObject;
    private EntityLife entityLife;
    private PlayerHealth playerHealth;
    private LifeType playerLifeType;

    [SetUp]
    public void SetUp()
    {
        playerLifeType = ScriptableObject.CreateInstance<LifeType>();
        playerLifeType.entityName = "PlayerLifeType";
        playerLifeType.maxHealth = ExpectedMaxHealth;
        playerLifeType.hasShield = true;
        playerLifeType.shieldHealth = ExpectedBaseShield;

        playerObject = new GameObject("Player_TC005");
        playerObject.tag = "Player";
        entityLife = playerObject.AddComponent<EntityLife>();
        playerHealth = playerObject.AddComponent<PlayerHealth>();
        playerHealth.playerLifeType = playerLifeType;
    }

    [TearDown]
    public void TearDown()
    {
        if (playerObject != null) Object.DestroyImmediate(playerObject);
        if (playerLifeType != null) Object.DestroyImmediate(playerLifeType);
    }

    [Test]
    [Description("TC-005: Verificar que al inicializar la nave del jugador comienza con 100 de vida y 100 de escudo base.")]
    public void TC005_IniciarNaveJugador_ComienzaCon100DeVidaY100DeEscudo()
    {
        playerHealth.InitializeHealth();

        Assert.AreEqual(ExpectedMaxHealth, playerHealth.maxHealth,
            "La vida máxima del jugador debe ser 100.");
        Assert.AreEqual(ExpectedMaxHealth, playerHealth.currentHealth,
            "La nave debe iniciar con 100 puntos de vida actual.");
        Assert.AreEqual(ExpectedBaseShield, entityLife.CurrentShieldHealth,
            "La nave debe iniciar con 100 puntos de escudo base.");
        Assert.IsTrue(entityLife.IsShieldActive,
            "El escudo base debe encontrarse activo al iniciar con PlayerLife.");
    }

    [Test]
    [Description("TC-005 (PE + Branch Coverage): Verificar inicialización con LifeType con escudo, sin escudo y null.")]
    public void TC005_ParticionEquivalencias_ConfiguracionesDeLifeType()
    {
        entityLife.lifeType = playerLifeType;
        entityLife.InitializeFromLifeType();
        Assert.AreEqual(100, entityLife.CurrentHealth);
        Assert.AreEqual(100, entityLife.CurrentShieldHealth);
        Assert.IsTrue(entityLife.IsShieldActive);

        playerLifeType.hasShield = false;
        entityLife.InitializeFromLifeType();
        Assert.AreEqual(100, entityLife.CurrentHealth);
        Assert.AreEqual(0, entityLife.CurrentShieldHealth);
        Assert.IsFalse(entityLife.IsShieldActive);

        entityLife.lifeType = null;
        entityLife.InitializeFromLifeType();
        Assert.AreEqual(100, entityLife.CurrentHealth);
        Assert.AreEqual(0, entityLife.CurrentShieldHealth);
        Assert.IsFalse(entityLife.IsShieldActive);
    }
}
