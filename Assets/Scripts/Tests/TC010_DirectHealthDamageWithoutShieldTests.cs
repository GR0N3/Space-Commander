using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("VidaDelJugador")]
[Category("TablaDeDecision")]
public class TC010_DirectHealthDamageWithoutShieldTests
{
    private const int InitialPlayerHealth = 100;
    private const int EnemyBulletDamage = 15;

    private GameObject playerObject;
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
        playerLifeType.shieldHealth = 100;

        playerObject = new GameObject("Player_TC010");
        playerObject.tag = "Player";
        playerCollider = playerObject.AddComponent<SphereCollider>();
        playerEntityLife = playerObject.AddComponent<EntityLife>();
        playerEntityLife.lifeType = playerLifeType;
        playerHealth = playerObject.AddComponent<PlayerHealth>();
        playerHealth.playerLifeType = playerLifeType;
        playerHealth.InitializeHealth();
        playerEntityLife.DeactivateShield();

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
    [Description("TC-010: Con escudo en 0 y vida en 100, al recibir impacto de bala enemiga (15) la vida baja directamente a 85.")]
    public void TC010_RecibirImpactoBalaEnemiga_ConEscudoEnCero_VidaBajaDirectamente15Puntos()
    {
        Assert.AreEqual(0, playerEntityLife.CurrentShieldHealth, "Precondición: el escudo debe estar en 0.");
        Assert.IsFalse(playerEntityLife.IsShieldActive, "Precondición: el escudo debe estar inactivo.");

        var bulletGo = new GameObject("EnemyBullet_TC010");
        bulletGo.AddComponent<SphereCollider>();
        bulletGo.AddComponent<Rigidbody>();
        var enemyBullet = bulletGo.AddComponent<Bullet>();
        enemyBullet.Initialize(enemyBulletData);

        try
        {
            enemyBullet.SimulateHit(playerCollider);

            Assert.AreEqual(0, playerEntityLife.CurrentShieldHealth, "El escudo debe permanecer en 0.");
            Assert.AreEqual(InitialPlayerHealth - EnemyBulletDamage, playerHealth.currentHealth,
                "Sin escudo, la vida debe bajar directamente 15 puntos (quedando en 85).");
        }
        finally
        {
            if (bulletGo != null) Object.DestroyImmediate(bulletGo);
        }
    }

    [TestCase(true, 100, 15, 85, 100, TestName = "TC010_TablaDecision_Regla1_Escudo100_AbsorbeTodo")]
    [TestCase(true, 10, 15, 0, 95, TestName = "TC010_TablaDecision_Regla2_Escudo10_PasaExcedente5")]
    [TestCase(false, 0, 15, 0, 85, TestName = "TC010_TablaDecision_Regla3_SinEscudo_DanioDirecto15")]
    public void TC010_TablaDeDecision_TodasLasReglasREQ008(
        bool shieldActive, int initialShield, int damage, int expectedShield, int expectedHealth)
    {
        playerHealth.InitializeHealth();
        if (shieldActive)
        {
            playerEntityLife.ActivateShield(initialShield);
        }
        else
        {
            playerEntityLife.DeactivateShield();
        }

        playerHealth.TakeDamage(damage);

        Assert.AreEqual(expectedShield, playerEntityLife.CurrentShieldHealth);
        Assert.AreEqual(expectedHealth, playerHealth.currentHealth);
    }
}
