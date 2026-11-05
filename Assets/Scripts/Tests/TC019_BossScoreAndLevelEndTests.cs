using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("JefeFinal")]
[Category("TablaDeDecision")]
public class TC019_BossScoreAndLevelEndTests
{
    private const int PlayerBulletDamage = 50;
    private const int BossInitialHealth = 100;
    private const int ExpectedPointsPerBossHit = 250;

    private GameObject bossObject;
    private Boss bossComponent;
    private EntityLife bossEntityLife;
    private EnemyLife bossEnemyLife;
    private BoxCollider bossCollider;
    private LifeType bossLifeType;

    private GameObject lifeFacadeObject;
    private LifeFacade lifeFacade;

    private GameObject flowObject;
    private GameFlowFacade gameFlow;
    private GameObject levelCompletePanel;

    private BulletData playerBulletData;
    private BulletData enemyBulletData;

    [SetUp]
    public void SetUp()
    {
        ScoreManager.Instance.ResetScore();

        levelCompletePanel = new GameObject("LevelCompletePanel_TC019");
        levelCompletePanel.SetActive(false);

        flowObject = new GameObject("GameFlowFacade_TC019");
        gameFlow = flowObject.AddComponent<GameFlowFacade>();
        gameFlow.entryFadeOnStart = false;
        gameFlow.showLevelCompleteOnEnd = true;
        gameFlow.levelCompleteUI = levelCompletePanel;
        gameFlow.levelCompleteDelaySeconds = 10f;
        gameFlow.StartLevel();

        lifeFacadeObject = new GameObject("LifeFacade_TC019");
        lifeFacade = lifeFacadeObject.AddComponent<LifeFacade>();

        bossLifeType = ScriptableObject.CreateInstance<LifeType>();
        bossLifeType.entityName = "Boss";
        bossLifeType.maxHealth = BossInitialHealth;
        bossLifeType.hasShield = false;
        bossLifeType.pointsOnDeath = 0;

        bossObject = new GameObject("Boss_TC019");
        bossObject.tag = "Boss";
        bossCollider = bossObject.AddComponent<BoxCollider>();
        bossEntityLife = bossObject.AddComponent<EntityLife>();
        bossEntityLife.lifeType = bossLifeType;
        bossEntityLife.lifeFacade = lifeFacade;
        bossEnemyLife = bossObject.AddComponent<EnemyLife>();
        bossEnemyLife.enemyLifeType = bossLifeType;
        bossEnemyLife.InitializeEnemyLife();
        bossComponent = bossObject.AddComponent<Boss>();
        bossComponent.health = BossInitialHealth;

        playerBulletData = ScriptableObject.CreateInstance<BulletData>();
        playerBulletData.damage = PlayerBulletDamage;
        playerBulletData.ownerTag = "Player";
        playerBulletData.speed = 50f;
        playerBulletData.lifeTime = 5f;

        enemyBulletData = ScriptableObject.CreateInstance<BulletData>();
        enemyBulletData.damage = 15;
        enemyBulletData.ownerTag = "Enemy";
        enemyBulletData.speed = 30f;
        enemyBulletData.lifeTime = 5f;
    }

    [TearDown]
    public void TearDown()
    {
        ScoreManager.Instance.ResetScore();
        if (bossObject != null) Object.DestroyImmediate(bossObject);
        if (lifeFacadeObject != null) Object.DestroyImmediate(lifeFacadeObject);
        if (flowObject != null) Object.DestroyImmediate(flowObject);
        if (levelCompletePanel != null) Object.DestroyImmediate(levelCompletePanel);
        if (bossLifeType != null) Object.DestroyImmediate(bossLifeType);
        if (playerBulletData != null) Object.DestroyImmediate(playerBulletData);
        if (enemyBulletData != null) Object.DestroyImmediate(enemyBulletData);
    }

    private Bullet CreateBullet(BulletData data)
    {
        var go = new GameObject("Bullet_TC019");
        go.AddComponent<SphereCollider>();
        go.AddComponent<Rigidbody>();
        var bullet = go.AddComponent<Bullet>();
        bullet.Initialize(data);
        return bullet;
    }

    [Test]
    [Description("TC-019: Verificar que cada impacto de bala del jugador al Boss sume 250 puntos y al llegar su vida a 0 finalice el nivel.")]
    public void TC019_ImpactarYDerrotarAlBoss_Suma250PuntosPorImpactoYDisparaFinDeNivel()
    {
        Assert.AreEqual(0, ScoreManager.Instance.Score, "Precondición: el puntaje inicial debe ser 0.");
        Assert.AreEqual(BossInitialHealth, bossEntityLife.CurrentHealth, "Precondición: vida inicial del Boss = 100.");
        Assert.IsFalse(levelCompletePanel.activeSelf, "Precondición: levelCompleteUI debe estar inactivo.");

        Bullet bullet1 = CreateBullet(playerBulletData);
        Bullet bullet2 = CreateBullet(playerBulletData);

        try
        {
            // Paso 1: Primer impacto no letal sobre el Boss (100 - 50 = 50 de vida restante)
            bullet1.SimulateHit(bossCollider);

            Assert.AreEqual(50, bossEntityLife.CurrentHealth,
                "Tras el 1er impacto (50 daño), la vida del Boss debe quedar en 50.");
            Assert.AreEqual(ExpectedPointsPerBossHit, ScoreManager.Instance.Score,
                "El 1er impacto de bala del jugador sobre el Boss debe sumar 250 puntos.");
            Assert.IsTrue(bossObject.activeSelf,
                "Con vida > 0 (50), el Boss debe seguir activo.");
            Assert.IsFalse(levelCompletePanel.activeSelf,
                "Con el Boss aún con vida, el nivel no debe finalizar.");

            // Paso 2: Segundo impacto letal sobre el Boss (50 - 50 = 0 de vida)
            bullet2.SimulateHit(bossCollider);

            Assert.AreEqual(0, bossEntityLife.CurrentHealth,
                "Tras el 2do impacto, la vida del Boss debe llegar a 0.");
            Assert.AreEqual(ExpectedPointsPerBossHit * 2, ScoreManager.Instance.Score,
                "El 2do impacto debe sumar otros 250 puntos (total = 500 puntos).");
            Assert.IsFalse(bossObject.activeSelf,
                "Al llegar su vida a 0, el Boss debe desactivarse/destruirse.");
            Assert.IsTrue(levelCompletePanel.activeSelf,
                "Al morir el Boss, LifeFacade debe invocar GameFlowFacade.EndLevel() activando levelCompleteUI.");
        }
        finally
        {
            if (bullet1 != null) Object.DestroyImmediate(bullet1.gameObject);
            if (bullet2 != null) Object.DestroyImmediate(bullet2.gameObject);
        }
    }

    [TestCase("Player", true, 50, 250, false, TestName = "TC019_TablaDecision_R1_BalaPlayer_ImpactoNoLetal_Suma250YContinuaNivel")]
    [TestCase("Player", true, 100, 250, true, TestName = "TC019_TablaDecision_R2_BalaPlayer_ImpactoLetal_Suma250YFinalizaNivel")]
    [TestCase("Enemy", true, 50, 0, false, TestName = "TC019_TablaDecision_R3_BalaEnemigaContraBoss_IgnoraImpactoYSuma0")]
    public void TC019_TablaDeDecision_CondicionesPropietarioBalaEntidadBossYMuerte(
        string ownerTag,
        bool targetIsBoss,
        int bulletDamage,
        int expectedScoreDelta,
        bool expectedLevelEnded)
    {
        ScoreManager.Instance.ResetScore();
        levelCompletePanel.SetActive(false);
        bossEntityLife.InitializeFromLifeType();
        bossObject.SetActive(true);

        var testBulletData = ScriptableObject.CreateInstance<BulletData>();
        testBulletData.ownerTag = ownerTag;
        testBulletData.damage = bulletDamage;
        testBulletData.speed = 50f;
        testBulletData.lifeTime = 5f;

        Bullet bullet = CreateBullet(testBulletData);

        try
        {
            bullet.SimulateHit(bossCollider);

            Assert.AreEqual(expectedScoreDelta, ScoreManager.Instance.Score);
            Assert.AreEqual(expectedLevelEnded, levelCompletePanel.activeSelf);
        }
        finally
        {
            if (bullet != null) Object.DestroyImmediate(bullet.gameObject);
            if (testBulletData != null) Object.DestroyImmediate(testBulletData);
        }
    }
}
