using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("VidaDelJugador")]
[Category("AnalisisDeValoresLimite")]
public class TC009_ShieldOverflowDamageTests
{
    private const int InitialPlayerHealth = 100;
    private const int PartialShieldPoints = 10;
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

        playerObject = new GameObject("Player_TC009");
        playerObject.tag = "Player";
        playerCollider = playerObject.AddComponent<SphereCollider>();
        playerEntityLife = playerObject.AddComponent<EntityLife>();
        playerEntityLife.lifeType = playerLifeType;
        playerHealth = playerObject.AddComponent<PlayerHealth>();
        playerHealth.playerLifeType = playerLifeType;
        playerHealth.InitializeHealth();
        playerEntityLife.ActivateShield(PartialShieldPoints);

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
    [Description("TC-009: Con escudo en 10 y vida en 100, al recibir 15 de daño de bala enemiga el escudo queda en 0 y la vida baja 5 puntos (a 95).")]
    public void TC009_RecibirImpactoBalaEnemiga_ConEscudoMenorAlDanio_EscudoEnCeroYVidaBajaExcedente()
    {
        int previousShield = playerEntityLife.CurrentShieldHealth;
        int expectedOverflow = EnemyBulletDamage - previousShield;
        int expectedHealth = InitialPlayerHealth - expectedOverflow;

        var bulletGo = new GameObject("EnemyBullet_TC009");
        bulletGo.AddComponent<SphereCollider>();
        bulletGo.AddComponent<Rigidbody>();
        var enemyBullet = bulletGo.AddComponent<Bullet>();
        enemyBullet.Initialize(enemyBulletData);

        try
        {
            enemyBullet.SimulateHit(playerCollider);

            Assert.AreEqual(0, playerEntityLife.CurrentShieldHealth,
                "El escudo debe quedar en 0 tras absorber su capacidad restante.");
            Assert.IsFalse(playerEntityLife.IsShieldActive,
                "El escudo debe desactivarse al romperse (llegar a 0).");
            Assert.AreEqual(expectedHealth, playerHealth.currentHealth,
                "La vida debe bajar exactamente el excedente (15 - 10 = 5, quedando en 95).");
        }
        finally
        {
            if (bulletGo != null) Object.DestroyImmediate(bulletGo);
        }
    }

    [TestCase(1, 0, false, 86, TestName = "TC009_AVL_LimiteInferior_Escudo1_Excedente14_Vida86")]
    [TestCase(14, 0, false, 99, TestName = "TC009_AVL_LimiteSuperior_Escudo14_Excedente1_Vida99")]
    [TestCase(15, 0, false, 100, TestName = "TC009_AVL_VecinoExacto_Escudo15_Excedente0_Vida100")]
    [TestCase(16, 1, true, 100, TestName = "TC009_AVL_VecinoSinRomper_Escudo16_Escudo1_Vida100")]
    public void TC009_AnalisisValoresLimite_ExcedenteDeEscudo(int shieldBefore, int expectedShieldAfter, bool expectedShieldActive, int expectedHealthAfter)
    {
        playerHealth.InitializeHealth();
        playerEntityLife.ActivateShield(shieldBefore);

        playerHealth.TakeDamage(EnemyBulletDamage);

        Assert.AreEqual(expectedShieldAfter, playerEntityLife.CurrentShieldHealth);
        Assert.AreEqual(expectedShieldActive, playerEntityLife.IsShieldActive);
        Assert.AreEqual(expectedHealthAfter, playerHealth.currentHealth);
    }
}
