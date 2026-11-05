using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("Pausa")]
[Category("TransicionDeEstados")]
public class TC013_ResumeOnAnyKeyTests
{
    private GameObject pauseMenuObject;
    private PauseMenu pauseMenu;
    private GameObject pausePanelObject;

    [SetUp]
    public void SetUp()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneTransition.ResetLastLoadedSceneTracking();

        pausePanelObject = new GameObject("PausePanel_TC013");
        pausePanelObject.SetActive(false);

        pauseMenuObject = new GameObject("PauseMenu_TC013");
        pauseMenu = pauseMenuObject.AddComponent<PauseMenu>();
        pauseMenu.Panel = pausePanelObject;

        pauseMenu.SimulateKeyPress(KeyCode.Escape);
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneTransition.ResetLastLoadedSceneTracking();

        if (pauseMenuObject != null) Object.DestroyImmediate(pauseMenuObject);
        if (pausePanelObject != null) Object.DestroyImmediate(pausePanelObject);
    }

    [Test]
    [Description("TC-013: Con el juego pausado, presionar la barra espaciadora reanuda la partida.")]
    public void TC013_ConJuegoPausado_PresionarBarraEspaciadora_ReanudaLaPartida()
    {
        Assert.IsTrue(pauseMenu.IsPaused, "Precondición: el juego debe estar pausado.");
        Assert.AreEqual(0f, Time.timeScale, "Precondición: Time.timeScale debe ser 0f.");
        Assert.IsTrue(pausePanelObject.activeSelf, "Precondición: el panel de pausa debe estar activo.");

        pauseMenu.SimulateKeyPress(KeyCode.Space);

        Assert.IsFalse(pauseMenu.IsPaused, "El juego debe haber salido del estado de pausa.");
        Assert.AreEqual(1f, Time.timeScale, "Time.timeScale debe restaurarse a 1f para continuar la partida.");
        Assert.IsFalse(AudioListener.pause, "El audio debe reanudarse (AudioListener.pause = false).");
        Assert.IsFalse(pausePanelObject.activeSelf, "El panel de pausa debe ocultarse al reanudar.");
    }

    [TestCase(KeyCode.Space, TestName = "TC013_TransicionEstados_PausadoMasEspacio_Reanuda")]
    [TestCase(KeyCode.W, TestName = "TC013_TransicionEstados_PausadoMasW_Reanuda")]
    [TestCase(KeyCode.Return, TestName = "TC013_TransicionEstados_PausadoMasEnter_Reanuda")]
    public void TC013_TransicionDeEstados_CualquierTeclaDistintaDeEsc_ReanudaPartida(KeyCode resumeKey)
    {
        pauseMenu.SimulateKeyPress(resumeKey);

        Assert.IsFalse(pauseMenu.IsPaused);
        Assert.AreEqual(1f, Time.timeScale);
    }

    [Test]
    [Description("TC-013 (Flujo Alternativo CU-02 - Transición de Estados): Estando en pausa, presionar Esc carga el menú principal.")]
    public void TC013_TransicionDeEstados_PausadoMasEsc_CargaMenuPrincipal()
    {
        pauseMenu.SimulateKeyPress(KeyCode.Escape);

        Assert.AreEqual("Menu", SceneTransition.LastLoadedSceneName);
        Assert.AreEqual(1f, Time.timeScale);
    }
}
