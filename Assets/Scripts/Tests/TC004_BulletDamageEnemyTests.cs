using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("Disparo")]
[Category("AnalisisDeValoresLimite")]
public class TC004_BulletDamageEnemyTests
{
    private const int PlayerBulletDamage = 50;
    private const int EnemyInitialHealth = 100;

    private GameObject enemyObject;
    private EnemyLife enemyLife;
    private EntityLife enemyEntityLife;
    private SphereCollider enemyCollider;
    private LifeType enemyLifeType;
    private BulletData playerBulletData;
    private GameObject lifeFacadeObject;
    private LifeFacade lifeFacade;

    [SetUp]
    public void SetUp()
    {
        lifeFacadeObject = new GameObject("LifeFacade_TC004");
        lifeFacade = lifeFacadeObject.AddComponent<LifeFacade>();

        enemyLifeType = ScriptableObject.CreateInstance<LifeType>();
        enemyLifeType.entityName = "EnemyLifeType";
        enemyLifeType.maxHealth = EnemyInitialHealth;
        enemyLifeType.hasShield = false;
        enemyLifeType.pointsOnDeath = 0;

        enemyObject = new GameObject("Enemy00_TC004");
        enemyObject.tag = "Enemy";
        enemyCollider = enemyObject.AddComponent<SphereCollider>();
        enemyEntityLife = enemyObject.AddComponent<EntityLife>();
        enemyEntityLife.lifeType = enemyLifeType;
        enemyEntityLife.lifeFacade = lifeFacade;
        enemyLife = enemyObject.AddComponent<EnemyLife>();
        enemyLife.enemyLifeType = enemyLifeType;
        enemyLife.InitializeEnemyLife();

        playerBulletData = ScriptableObject.CreateInstance<BulletData>();
        playerBulletData.damage = PlayerBulletDamage;
        playerBulletData.ownerTag = "Player";
        playerBulletData.speed = 50f;
        playerBulletData.lifeTime = 5f;
    }

    [TearDown]
    public void TearDown()
    {
        if (enemyObject != null) Object.DestroyImmediate(enemyObject);
        if (lifeFacadeObject != null) Object.DestroyImmediate(lifeFacadeObject);
        if (enemyLifeType != null) Object.DestroyImmediate(enemyLifeType);
        if (playerBulletData != null) Object.DestroyImmediate(playerBulletData);
    }

    private Bullet CreatePlayerBullet()
    {
        var bulletGo = new GameObject("PlayerBullet_TC004");
        bulletGo.AddComponent<SphereCollider>();
        bulletGo.AddComponent<Rigidbody>();
        var bullet = bulletGo.AddComponent<Bullet>();
        bullet.Initialize(playerBulletData);
        return bullet;
    }

    [Test]
    [Description("TC-004: Tras 1 impacto de bala (50 daño) el enemigo queda vivo con 50 de vida; tras 2 impactos muere.")]
    public void TC004_ImpactarEnemigoConDosBalas_1ImpactoQuedaEn50_2ImpactosMuere()
    {
        Assert.AreEqual(EnemyInitialHealth, enemyLife.CurrentHealth, "El enemigo debe iniciar con 100 de vida.");
        Bullet bullet1 = CreatePlayerBullet();
        Bullet bullet2 = CreatePlayerBullet();

        try
        {
            bullet1.SimulateHit(enemyCollider);

            Assert.AreEqual(50, enemyLife.CurrentHealth,
                "Tras 1 impacto de 50 de daño, el enemigo debe quedar con 50 puntos de vida.");
            Assert.IsTrue(enemyObject.activeSelf,
                "Tras 1 impacto, el enemigo debe seguir vivo (activo).");

            bullet2.SimulateHit(enemyCollider);

            Assert.AreEqual(0, enemyLife.CurrentHealth,
                "Tras 2 impactos de 50 de daño, la vida del enemigo debe llegar a 0.");
        }
        finally
        {
            if (bullet1 != null) Object.DestroyImmediate(bullet1.gameObject);
            if (bullet2 != null) Object.DestroyImmediate(bullet2.gameObject);
        }
    }

    [TestCase(51, 1, true, TestName = "TC004_AVL_Vida51_Impacto50_QuedaEn1SigueVivo")]
    [TestCase(50, 0, false, TestName = "TC004_AVL_Vida50_Impacto50_QuedaEn0Muere")]
    [TestCase(49, 0, false, TestName = "TC004_AVL_Vida49_Impacto50_ClampEn0Muere")]
    public void TC004_AnalisisValoresLimite_UmbralDeMuerteEnemigo(int initialHealth, int expectedRemainingHealth, bool expectedAlive)
    {
        enemyLifeType.maxHealth = initialHealth;
        enemyLife.InitializeEnemyLife();
        Bullet bullet = CreatePlayerBullet();

        try
        {
            bullet.SimulateHit(enemyCollider);

            Assert.AreEqual(expectedRemainingHealth, enemyLife.CurrentHealth);
            Assert.AreEqual(expectedAlive, enemyLife.CurrentHealth > 0);
        }
        finally
        {
            if (bullet != null) Object.DestroyImmediate(bullet.gameObject);
        }
    }

    [Test]
    [Description("TC-004 (PPT Diapositiva 10 - PE no-numérica): Bala del jugador daña a Enemigo pero no daña a Aliado/Player.")]
    public void TC004_ParticionEquivalenciasNoNumerica_DisparoContraEnemigoVsAliado()
    {
        var allyObject = new GameObject("PlayerAlly_TC004");
        allyObject.tag = "Player";
        var allyCollider = allyObject.AddComponent<SphereCollider>();
        var allyLife = allyObject.AddComponent<EntityLife>();
        allyLife.lifeType = enemyLifeType;
        allyLife.InitializeFromLifeType();

        Bullet bulletAgainstAlly = CreatePlayerBullet();
        Bullet bulletAgainstEnemy = CreatePlayerBullet();

        try
        {
            bulletAgainstAlly.SimulateHit(allyCollider);
            bulletAgainstEnemy.SimulateHit(enemyCollider);

            Assert.AreEqual(100, allyLife.CurrentHealth,
                "La bala del jugador no debe dañar a un objeto aliado con tag 'Player'.");
            Assert.AreEqual(50, enemyLife.CurrentHealth,
                "La bala del jugador sí debe restar 50 de vida a un objeto con tag 'Enemy'.");
        }
        finally
        {
            if (allyObject != null) Object.DestroyImmediate(allyObject);
            if (bulletAgainstAlly != null) Object.DestroyImmediate(bulletAgainstAlly.gameObject);
            if (bulletAgainstEnemy != null) Object.DestroyImmediate(bulletAgainstEnemy.gameObject);
        }
    }
}
