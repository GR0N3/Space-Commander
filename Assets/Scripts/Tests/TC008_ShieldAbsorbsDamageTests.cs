using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("VidaDelJugador")]
[Category("TablaDeDecision")]
public class TC008_ShieldAbsorbsDamageTests
{
    private const int InitialPlayerHealth = 100;
    private const int BaseShieldPoints = 100;
    private const int EnemyBulletDamage = 15;

    private GameObject playerObject;
    private PlayerFacade playerFacade;
    private EntityLife playerEntityLife;
    private PlayerHealth playerHealth;
    private SphereCollider playerCollider;
    private LifeType playerLifeType;
    private BulletData enemyBulletData;

    [SetUp]
    public void SetUp()
    {
        playerLifeType = ScriptableObject.CreateInstance<LifeType>();
        playerLifeType.entityName = "PlayerLifeType";
        playerLifeType.maxHealth = InitialPlayerHealth;
        playerLifeType.hasShield = true;
        playerLifeType.shieldHealth = BaseShieldPoints;

        playerObject = new GameObject("Player_TC008");
        playerObject.tag = "Player";
        playerCollider = playerObject.AddComponent<SphereCollider>();
        playerEntityLife = playerObject.AddComponent<EntityLife>();
        playerEntityLife.lifeType = playerLifeType;
        playerHealth = playerObject.AddComponent<PlayerHealth>();
        playerHealth.playerLifeType = playerLifeType;
        playerHealth.InitializeHealth();
        playerEntityLife.ActivateShield(BaseShieldPoints);

        playerFacade = playerObject.AddComponent<PlayerFacade>();
        playerFacade.playerHealth = playerHealth;

        enemyBulletData = ScriptableObject.CreateInstance<BulletData>();
        enemyBulletData.damage = EnemyBulletDamage;
        enemyBulletData.ownerTag = "Enemy";
        enemyBulletData.speed = 25f;
        enemyBulletData.lifeTime = 5f;
    }

    [TearDown]
    public void TearDown()
    {
        if (playerObject != null) Object.DestroyImmediate(playerObject);
        if (playerLifeType != null) Object.DestroyImmediate(playerLifeType);
        if (enemyBulletData != null) Object.DestroyImmediate(enemyBulletData);
    }

    [Test]
    [Description("TC-008: Con escudo en 100 y vida en 100, al recibir impacto de bala enemiga (15), el escudo baja a 85 y la vida sigue en 100.")]
    public void TC008_RecibirImpactoBalaEnemiga_ConEscudoMayorAlDanio_EscudoBaja15YVidaSeMantieneEn100()
    {
        var bulletGo = new GameObject("EnemyBullet_TC008");
        bulletGo.AddComponent<SphereCollider>();
        bulletGo.AddComponent<Rigidbody>();
        var enemyBullet = bulletGo.AddComponent<Bullet>();
        enemyBullet.Initialize(enemyBulletData);

        try
        {
            enemyBullet.SimulateHit(playerCollider);

            Assert.AreEqual(BaseShieldPoints - EnemyBulletDamage, playerEntityLife.CurrentShieldHealth,
                "El escudo debe bajar 15 puntos (de 100 a 85).");
            Assert.AreEqual(InitialPlayerHealth, playerHealth.currentHealth,
                "La vida del jugador debe mantenerse intacta en 100.");
            Assert.IsTrue(playerEntityLife.IsShieldActive,
                "El escudo debe seguir activo al quedarle 85 puntos de resistencia.");
        }
        finally
        {
            if (bulletGo != null) Object.DestroyImmediate(bulletGo);
        }
    }

    [TestCase(100, 15, 85, 100, TestName = "TC008_TablaDecision_R1_Escudo100_Danio15_Escudo85_Vida100")]
    [TestCase(50, 15, 35, 100, TestName = "TC008_TablaDecision_R1_Escudo50_Danio15_Escudo35_Vida100")]
    [TestCase(16, 15, 1, 100, TestName = "TC008_TablaDecision_R1_Escudo16_Danio15_Escudo1_Vida100")]
    public void TC008_TablaDeDecision_ReglaEscudoMayorAlDanio(int initialShield, int damage, int expectedShield, int expectedHealth)
    {
        playerEntityLife.ActivateShield(initialShield);

        playerFacade.TakeDamage(damage);

        Assert.AreEqual(expectedShield, playerEntityLife.CurrentShieldHealth);
        Assert.AreEqual(expectedHealth, playerHealth.currentHealth);
        Assert.IsTrue(playerEntityLife.IsShieldActive);
    }
}
