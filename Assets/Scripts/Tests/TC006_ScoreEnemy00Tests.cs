using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("Puntaje")]
[Category("ParticionDeEquivalencias")]
public class TC006_ScoreEnemy00Tests
{
    private const int ExpectedEnemy00Points = 100;
    private const int EnemyHealth = 100;

    private GameObject enemy00Object;
    private EnemyLife enemyLife;
    private LifeType enemyLifeType;
    private GameObject lifeFacadeObject;
    private LifeFacade lifeFacade;

    [SetUp]
    public void SetUp()
    {
        ScoreManager.Instance.ResetScore();

        lifeFacadeObject = new GameObject("LifeFacade_TC006");
        lifeFacade = lifeFacadeObject.AddComponent<LifeFacade>();

        enemyLifeType = ScriptableObject.CreateInstance<LifeType>();
        enemyLifeType.entityName = "EnemyLifeType";
        enemyLifeType.maxHealth = EnemyHealth;
        enemyLifeType.hasShield = false;
        enemyLifeType.pointsOnDeath = 0;

        enemy00Object = new GameObject("Enemy00");
        enemy00Object.tag = "Enemy";
        var entityLife = enemy00Object.AddComponent<EntityLife>();
        entityLife.lifeType = enemyLifeType;
        entityLife.lifeFacade = lifeFacade;
        enemyLife = enemy00Object.AddComponent<EnemyLife>();
        enemyLife.enemyLifeType = enemyLifeType;
        enemyLife.InitializeEnemyLife();
    }

    [TearDown]
    public void TearDown()
    {
        ScoreManager.Instance.ResetScore();
        if (enemy00Object != null) Object.DestroyImmediate(enemy00Object);
        if (lifeFacadeObject != null) Object.DestroyImmediate(lifeFacadeObject);
        if (enemyLifeType != null) Object.DestroyImmediate(enemyLifeType);
    }

    [Test]
    [Description("TC-006: Al destruir un enemigo Enemy00 con puntaje inicial 0, el puntaje sube 100 puntos.")]
    public void TC006_DestruirEnemy00_ConPuntajeCero_PuntajeSube100Puntos()
    {
        Assert.AreEqual(0, ScoreManager.Instance.Score, "El puntaje inicial debe ser 0.");

        enemyLife.TakeDamage(EnemyHealth);

        Assert.AreEqual(0, enemyLife.CurrentHealth, "El Enemy00 debe haber sido destruido (vida = 0).");
        Assert.AreEqual(ExpectedEnemy00Points, ScoreManager.Instance.Score,
            "Al destruir un Enemy00, el puntaje debe subir 100 puntos.");
    }

    [TestCase(100, 100, TestName = "TC006_PE_PuntosValidosPositivos_100_Suma100")]
    [TestCase(0, 0, TestName = "TC006_PE_PuntosInvalidosCero_0_NoModificaPuntaje")]
    [TestCase(-100, 0, TestName = "TC006_PE_PuntosInvalidosNegativos_Menos100_NoModificaPuntaje")]
    public void TC006_ParticionEquivalencias_AddScoreValoresValidosEInvalidos(int pointsInput, int expectedScore)
    {
        ScoreManager.Instance.ResetScore();

        ScoreManager.Instance.AddScore(pointsInput);

        Assert.AreEqual(expectedScore, ScoreManager.Instance.Score);
    }
}
