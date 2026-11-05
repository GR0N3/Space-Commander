using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;

[TestFixture]
[Category("FlujoDeNivel")]
[Category("AnalisisDeValoresLimite")]
public class TC017_LevelCountdownTimerTests
{
    private const int DefaultCountdownSeconds = 120;

    private GameObject flowObject;
    private GameFlowFacade gameFlow;
    private GameObject levelCompletePanel;
    private GameObject timerTextObject;
    private TextMeshProUGUI timerText;
    private AudioSource musicSource;
    private AudioClip dummyClip;

    private FieldInfo timeLeftField;
    private FieldInfo timerRunningField;
    private MethodInfo updateMethod;

    [SetUp]
    public void SetUp()
    {
        levelCompletePanel = new GameObject("LevelCompleteUI_TC017");
        levelCompletePanel.SetActive(false);

        timerTextObject = new GameObject("TimerText_TC017");
        timerText = timerTextObject.AddComponent<TextMeshProUGUI>();

        flowObject = new GameObject("GameFlowFacade_TC017");
        musicSource = flowObject.AddComponent<AudioSource>();
        dummyClip = AudioClip.Create("DummyLevelMusic", 4410, 1, 4410, false);

        gameFlow = flowObject.AddComponent<GameFlowFacade>();
        gameFlow.countdownSeconds = DefaultCountdownSeconds;
        gameFlow.entryFadeOnStart = false;
        gameFlow.showLevelCompleteOnEnd = true;
        gameFlow.levelCompleteUI = levelCompletePanel;
        gameFlow.timerText = timerText;
        gameFlow.musicSource = musicSource;
        gameFlow.levelMusic = dummyClip;
        gameFlow.levelCompleteDelaySeconds = 10f;

        timeLeftField = typeof(GameFlowFacade).GetField("timeLeft", BindingFlags.NonPublic | BindingFlags.Instance);
        timerRunningField = typeof(GameFlowFacade).GetField("timerRunning", BindingFlags.NonPublic | BindingFlags.Instance);
        updateMethod = typeof(GameFlowFacade).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);

        gameFlow.StartLevel();
    }

    [TearDown]
    public void TearDown()
    {
        if (flowObject != null) Object.DestroyImmediate(flowObject);
        if (levelCompletePanel != null) Object.DestroyImmediate(levelCompletePanel);
        if (timerTextObject != null) Object.DestroyImmediate(timerTextObject);
        if (dummyClip != null) Object.DestroyImmediate(dummyClip);
    }

    private void SimulateTimerElapsed(float elapsedSeconds)
    {
        float currentTimeLeft = (float)timeLeftField.GetValue(gameFlow);
        float dt = gameFlow.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        timeLeftField.SetValue(gameFlow, currentTimeLeft - elapsedSeconds + dt);
        updateMethod.Invoke(gameFlow, null);
    }

    [Test]
    [Description("TC-017: Verificar que el temporizador inicie en 120 s y al llegar a 0 s detenga el contador y active levelCompleteUI.")]
    public void TC017_TemporizadorCuentaRegresivaLlegaACero_DetieneRelojYActivaPantallaNivelCompletado()
    {
        bool initialRunning = (bool)timerRunningField.GetValue(gameFlow);
        float initialTimeLeft = (float)timeLeftField.GetValue(gameFlow);

        Assert.IsTrue(initialRunning, "Precondición: el temporizador debe estar corriendo al iniciar el nivel.");
        Assert.AreEqual(120f, initialTimeLeft, 0.01f, "Precondición: el temporizador debe iniciar en 120 s.");
        Assert.AreEqual("120", timerText.text, "Precondición: el texto del temporizador debe mostrar 120.");
        Assert.IsFalse(levelCompletePanel.activeSelf, "Precondición: levelCompleteUI debe iniciar oculto.");

        // Paso 1: Descuento parcial (60 s transcurridos -> quedan 60 s)
        SimulateTimerElapsed(60f);
        Assert.AreEqual(60f, (float)timeLeftField.GetValue(gameFlow), 0.01f);
        Assert.AreEqual("60", timerText.text);
        Assert.IsTrue((bool)timerRunningField.GetValue(gameFlow), "Con 60 s restantes el nivel debe seguir en curso.");
        Assert.IsFalse(levelCompletePanel.activeSelf);

        // Paso 2: Transcurren los 60 s restantes hasta llegar exactamente a 0 s
        SimulateTimerElapsed(60f);

        Assert.AreEqual(0f, (float)timeLeftField.GetValue(gameFlow), 0.001f,
            "El tiempo restante debe quedar en 0 s.");
        Assert.AreEqual("0", timerText.text, "El texto del temporizador debe mostrar 0.");
        Assert.IsFalse((bool)timerRunningField.GetValue(gameFlow),
            "Al llegar a 0 s, timerRunning debe pasar a false.");
        Assert.IsFalse(musicSource.isPlaying,
            "Al finalizar el nivel, la música del nivel debe detenerse.");
        Assert.IsTrue(levelCompletePanel.activeSelf,
            "Al llegar el temporizador a 0 s, debe activarse la pantalla de nivel completado (levelCompleteUI).");
    }

    [TestCase(119.9f, true, false, TestName = "TC017_AVL_FronteraPositiva_0_1sRestantes_NivelContinua")]
    [TestCase(120.0f, false, true, TestName = "TC017_AVL_LimiteExacto_0_0sRestantes_FinalizaNivel")]
    [TestCase(120.5f, false, true, TestName = "TC017_AVL_VecinoDesborde_Menos0_5s_ClampEnCeroYFinalizaNivel")]
    public void TC017_AnalisisValoresLimite_FronteraCeroSegundosDelTemporizador(
        float totalElapsed,
        bool expectedTimerRunning,
        bool expectedLevelCompleteActive)
    {
        bool eventFired = false;
        gameFlow.onLevelEnd = new UnityEngine.Events.UnityEvent();
        gameFlow.onLevelEnd.AddListener(() => eventFired = true);

        SimulateTimerElapsed(totalElapsed);

        float remaining = (float)timeLeftField.GetValue(gameFlow);
        Assert.GreaterOrEqual(remaining, 0f, "timeLeft nunca debe quedar en un valor negativo.");
        Assert.AreEqual(expectedTimerRunning, (bool)timerRunningField.GetValue(gameFlow));
        Assert.AreEqual(expectedLevelCompleteActive, levelCompletePanel.activeSelf);
        Assert.AreEqual(expectedLevelCompleteActive, eventFired);
    }
}
