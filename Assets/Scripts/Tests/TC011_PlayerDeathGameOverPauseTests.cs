using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("VidaDelJugador")]
[Category("TransicionDeEstados")]
public class TC011_PlayerDeathGameOverPauseTests
{
    private const int EnemyBulletDamage = 15;

    private GameObject playerObject;
    private EntityLife playerEntityLife;
    private PlayerHealth playerHealth;
    private LifeType playerLifeType;
    private GameObject lifeFacadeObject;
    private LifeFacade lifeFacade;
    private GameObject gameOverPanel;

    [SetUp]
    public void SetUp()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        gameOverPanel = new GameObject("GameOverPanel_TC011");
        gameOverPanel.SetActive(false);

        lifeFacadeObject = new GameObject("LifeFacade_TC011");
        lifeFacade = lifeFacadeObject.AddComponent<LifeFacade>();
        lifeFacade.pauseOnPlayerDeath = true;
        lifeFacade.gameOverUI = gameOverPanel;

        playerLifeType = ScriptableObject.CreateInstance<LifeType>();
        playerLifeType.entityName = "PlayerLifeType";
        playerLifeType.maxHealth = 100;
        playerLifeType.hasShield = false;
        playerLifeType.shieldHealth = 0;

        playerObject = new GameObject("Player_TC011");
        playerObject.tag = "Player";
        playerEntityLife = playerObject.AddComponent<EntityLife>();
        playerEntityLife.lifeType = playerLifeType;
        playerEntityLife.lifeFacade = lifeFacade;
        playerHealth = playerObject.AddComponent<PlayerHealth>();
        playerHealth.playerLifeType = playerLifeType;
        playerHealth.InitializeHealth();
        playerEntityLife.DeactivateShield();

        playerHealth.TakeDamage(70);
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (playerObject != null) Object.DestroyImmediate(playerObject);
        if (lifeFacadeObject != null) Object.DestroyImmediate(lifeFacadeObject);
        if (gameOverPanel != null) Object.DestroyImmediate(gameOverPanel);
        if (playerLifeType != null) Object.DestroyImmediate(playerLifeType);
    }

    [Test]
    [Description("TC-011: Verificar la transición de estado de Partida en Curso a Juego Pausado (Game Over) cuando la vida llega a 0.")]
    public void TC011_RecibirImpactosHastaLlegarVidaACero_JuegoSePausaYActivaGameOver()
    {
        Assert.AreEqual(30, playerHealth.currentHealth, "Precondición: vida baja (30 puntos).");
        Assert.AreEqual(0, playerEntityLife.CurrentShieldHealth, "Precondición: escudo en 0.");
        Assert.AreEqual(1f, Time.timeScale, "Precondición: nivel en curso (Time.timeScale == 1).");

        playerHealth.TakeDamage(EnemyBulletDamage);
        Assert.AreEqual(15, playerHealth.currentHealth);
        Assert.AreEqual(1f, Time.timeScale, "Con vida > 0 (15), el juego debe continuar en curso.");
        Assert.IsTrue(playerObject.activeSelf, "La nave debe seguir activa mientras su vida sea mayor a 0.");

        playerHealth.TakeDamage(EnemyBulletDamage);

        Assert.AreEqual(0, playerHealth.currentHealth, "La vida del jugador debe haber llegado a 0.");
        Assert.AreEqual(0f, Time.timeScale,
            "Cuando la vida del jugador llega a 0, el juego debe pausarse (Time.timeScale == 0f).");
        Assert.IsTrue(AudioListener.pause,
            "El audio debe pausarse al entrar en estado Game Over.");
        Assert.IsFalse(playerObject.activeSelf,
            "La nave del jugador debe desactivarse al llegar su vida a 0.");
        Assert.IsTrue(gameOverPanel.activeSelf,
            "La interfaz de Game Over (esperando que el jugador presione una tecla) debe activarse.");
    }
}
