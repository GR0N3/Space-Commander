using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("Puntaje")]
[Category("ParticionDeEquivalencias")]
public class TC007_ScoreEnemy01Tests
{
    private const int ExpectedEnemy01Points = 350;
    private const int EnemyHealth = 100;

    private GameObject enemy01Object;
    private EnemyLife enemyLife;
    private LifeType enemyLifeType;
    private GameObject lifeFacadeObject;
    private LifeFacade lifeFacade;

    [SetUp]
    public void SetUp()
    {
        ScoreManager.Instance.ResetScore();

        lifeFacadeObject = new GameObject("LifeFacade_TC007");
        lifeFacade = lifeFacadeObject.AddComponent<LifeFacade>();

        enemyLifeType = ScriptableObject.CreateInstance<LifeType>();
        enemyLifeType.entityName = "EnemyLifeType";
        enemyLifeType.maxHealth = EnemyHealth;
        enemyLifeType.hasShield = false;
        enemyLifeType.pointsOnDeath = 0;

        enemy01Object = new GameObject("Enemy01");
        enemy01Object.tag = "Enemy";
        var entityLife = enemy01Object.AddComponent<EntityLife>();
        entityLife.lifeType = enemyLifeType;
        entityLife.lifeFacade = lifeFacade;
        enemyLife = enemy01Object.AddComponent<EnemyLife>();
        enemyLife.enemyLifeType = enemyLifeType;
        enemyLife.InitializeEnemyLife();
    }

    [TearDown]
    public void TearDown()
    {
        ScoreManager.Instance.ResetScore();
        if (enemy01Object != null) Object.DestroyImmediate(enemy01Object);
        if (lifeFacadeObject != null) Object.DestroyImmediate(lifeFacadeObject);
        if (enemyLifeType != null) Object.DestroyImmediate(enemyLifeType);
    }

    [Test]
    [Description("TC-007: Al destruir un enemigo Enemy01 con puntaje inicial 0, el puntaje sube 350 puntos.")]
    public void TC007_DestruirEnemy01_ConPuntajeCero_PuntajeSube350Puntos()
    {
        Assert.AreEqual(0, ScoreManager.Instance.Score, "El puntaje inicial debe ser 0.");

        enemyLife.TakeDamage(EnemyHealth);

        Assert.AreEqual(0, enemyLife.CurrentHealth, "El Enemy01 debe haber sido destruido (vida = 0).");
        Assert.AreEqual(ExpectedEnemy01Points, ScoreManager.Instance.Score,
            "Al destruir un Enemy01, el puntaje debe subir 350 puntos.");
    }

    [TestCase("Enemy00", 100, TestName = "TC007_PE_ClaseEnemy00_Otorga100Puntos")]
    [TestCase("Enemy01", 350, TestName = "TC007_PE_ClaseEnemy01_Otorga350Puntos")]
    [TestCase("NaveEnemigaAlien2", 350, TestName = "TC007_PE_ClaseAliasEnemy01_Otorga350Puntos")]
    [TestCase("Asteroid", 0, TestName = "TC007_PE_ClaseSinPuntos_Otorga0Puntos")]
    public void TC007_ParticionEquivalencias_ClasesDeEntidadEnemiga(string entityName, int expectedPoints)
    {
        ScoreManager.Instance.ResetScore();
        var tempEntity = new GameObject(entityName);
        tempEntity.tag = "Enemy";

        try
        {
            lifeFacade.HandleDeath(enemyLifeType, Vector3.zero, tempEntity);

            Assert.AreEqual(expectedPoints, ScoreManager.Instance.Score);
        }
        finally
        {
            if (tempEntity != null) Object.DestroyImmediate(tempEntity);
        }
    }
}
