using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("Pausa")]
[Category("TransicionDeEstados")]
public class TC012_PauseWithEscapeTests
{
    private GameObject pauseMenuObject;
    private PauseMenu pauseMenu;
    private GameObject pausePanelObject;

    [SetUp]
    public void SetUp()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        pausePanelObject = new GameObject("PausePanel_TC012");
        pausePanelObject.SetActive(false);

        pauseMenuObject = new GameObject("PauseMenu_TC012");
        pauseMenu = pauseMenuObject.AddComponent<PauseMenu>();
        pauseMenu.Panel = pausePanelObject;
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (pauseMenuObject != null) Object.DestroyImmediate(pauseMenuObject);
        if (pausePanelObject != null) Object.DestroyImmediate(pausePanelObject);
    }

    [Test]
    [Description("TC-012: Verificar que al presionar Esc durante la partida en curso el juego pasa al estado Pausado.")]
    public void TC012_PresionarEscConNivelEnCurso_ElJuegoSePausa()
    {
        Assert.IsFalse(pauseMenu.IsPaused, "Precondición: la partida no debe estar pausada.");
        Assert.AreEqual(1f, Time.timeScale, "Precondición: Time.timeScale debe ser 1f.");
        Assert.IsFalse(pausePanelObject.activeSelf, "Precondición: el panel de pausa debe estar oculto.");

        pauseMenu.SimulateKeyPress(KeyCode.Escape);

        Assert.IsTrue(pauseMenu.IsPaused, "El estado de PauseMenu debe ser IsPaused = true.");
        Assert.AreEqual(0f, Time.timeScale, "Time.timeScale debe ser 0f al pausar el juego.");
        Assert.IsTrue(AudioListener.pause, "AudioListener.pause debe activarse al pausar.");
        Assert.IsTrue(pausePanelObject.activeSelf, "El panel de pausa debe mostrarse al pausar.");
    }

    [TestCase(KeyCode.Space, TestName = "TC012_TransicionEstados_TeclaEspacioEnCurso_NoPausa")]
    [TestCase(KeyCode.W, TestName = "TC012_TransicionEstados_TeclaWEnCurso_NoPausa")]
    public void TC012_TransicionDeEstados_TeclaDistintaDeEscEnPartidaEnCurso_NoPausa(KeyCode key)
    {
        pauseMenu.SimulateKeyPress(key);

        Assert.IsFalse(pauseMenu.IsPaused);
        Assert.AreEqual(1f, Time.timeScale);
        Assert.IsFalse(pausePanelObject.activeSelf);
    }
}
