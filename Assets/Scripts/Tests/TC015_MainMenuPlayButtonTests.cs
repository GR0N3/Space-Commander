using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

[TestFixture]
[Category("Menu")]
[Category("ParticionDeEquivalencias")]
public class TC015_MainMenuPlayButtonTests
{
    private const int ExpectedLevel1BuildIndex = 1;

    private GameObject menuManagerObject;
    private MenuManager menuManager;

    [SetUp]
    public void SetUp()
    {
        SceneTransition.ResetLastLoadedSceneTracking();
        Time.timeScale = 0f;

        menuManagerObject = new GameObject("MenuManager_TC015");
        menuManager = menuManagerObject.AddComponent<MenuManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;
        SceneTransition.ResetLastLoadedSceneTracking();

        if (menuManagerObject != null)
        {
            Object.DestroyImmediate(menuManagerObject);
        }
    }

    [Test]
    [Description("TC-015: Verificar que al presionar el botón Jugar del menú principal (StartGame) se solicita cargar el Nivel 1.")]
    public void TC015_PresionarBotonJugarEnMenuPrincipal_CargaElNivel1()
    {
        menuManager.StartGame();

        Assert.AreEqual(1f, Time.timeScale,
            "Al iniciar la partida desde el menú principal, Time.timeScale debe establecerse en 1f.");
        Assert.IsNotNull(ScoreManager.Instance,
            "StartGame debe asegurar que la instancia de ScoreManager esté inicializada.");
        Assert.AreEqual(ExpectedLevel1BuildIndex, SceneTransition.LastLoadedSceneIndex,
            "El botón Jugar debe solicitar la carga de la escena con buildIndex = 1 (Nivel 1).");

        string scenePath = SceneUtility.GetScenePathByBuildIndex(ExpectedLevel1BuildIndex);
        Assert.That(scenePath, Does.Contain("Nivel(1)"),
            "El buildIndex 1 en Build Settings debe corresponder a la escena 'Nivel(1)'.");
    }

    [Test]
    [Description("TC-015 (PE): Verificar las particiones de navegación de MenuManager entre Jugar (Nivel 1) y Puntajes (Scores).")]
    public void TC015_ParticionEquivalencias_OpcionesDeNavegacionDelMenu()
    {
        menuManager.StartGame();
        Assert.AreEqual(1, SceneTransition.LastLoadedSceneIndex);

        menuManager.OpenScoreboard();
        Assert.AreEqual("Scores", SceneTransition.LastLoadedSceneName);
    }
}
