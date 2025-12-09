using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

public class GameFlowFacade : MonoBehaviour
{
    [Header("Audio & UI")]
    public AudioSource musicSource;
    public AudioClip levelMusic;

    [Header("UI Targets")]
    public MonoBehaviour progressUI;
    public MonoBehaviour statsUI;

    [Header("Countdown")]
    public int countdownSeconds = 120;
    public TextMeshProUGUI timerText;
    public string endSceneName;
    public bool pauseOnEndIfNoScene = true;
    public bool autoStartLevel = true;
    public bool useUnscaledTime = false;
    private float timeLeft;
    private bool timerRunning;
    [Header("Transitions")]
    public bool entryFadeOnStart = true;
    public float entryFadeDuration = 0.5f;
    [Header("End UI")]
    public bool showLevelCompleteOnEnd = true;
    public GameObject levelCompleteUI;
    public TextMeshProUGUI levelCompleteScoreText;
    public string levelCompleteFormat = "Puntos: {0}";
    public float levelCompleteDelaySeconds = 2f;

    public UnityEvent onLevelStart;
    public UnityEvent onLevelEnd;

    public void StartLevel()
    {
        if (entryFadeOnStart)
        {
            SceneTransition.FadeInFromBlack(entryFadeDuration);
        }
        if (musicSource != null && levelMusic != null)
        {
            musicSource.clip = levelMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
        timeLeft = countdownSeconds;
        timerRunning = true;
        UpdateTimerDisplay();
        onLevelStart?.Invoke();
    }

    public void EndLevel()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
        timerRunning = false;
        onLevelEnd?.Invoke();

        if (showLevelCompleteOnEnd && levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(true);
            if (levelCompleteScoreText != null)
            {
                var sm = ScoreManager.Instance;
                levelCompleteScoreText.text = string.Format(levelCompleteFormat, sm != null ? sm.Score : 0);
            }
        }

        StartCoroutine(ProceedToNextSceneAfterDelay());
    }

    private System.Collections.IEnumerator ProceedToNextSceneAfterDelay()
    {
        float delay = Mathf.Max(0f, levelCompleteDelaySeconds);
        float t = 0f;
        while (t < delay)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        string current = SceneManager.GetActiveScene().name;
        if (!string.IsNullOrEmpty(endSceneName) && endSceneName != current)
        {
            SceneTransition.LoadScene(endSceneName);
            yield break;
        }

        string next = "Scores";
        if (!string.IsNullOrEmpty(next))
        {
            SceneTransition.LoadScene(next);
        }
        else if (pauseOnEndIfNoScene)
        {
            Time.timeScale = 0f;
        }
    }

    public void SetProgress(float normalized)
    {
        if (progressUI != null)
            progressUI.SendMessage("SetProgress", Mathf.Clamp01(normalized), SendMessageOptions.DontRequireReceiver);
    }

    public void AddGlobalStat(string key, float value)
    {
        if (statsUI != null)
            statsUI.SendMessage("AddGlobalStat", new object[] { key, value }, SendMessageOptions.DontRequireReceiver);
    }

    void Start()
    {
        if (autoStartLevel)
        {
            StartLevel();
        }
    }

    void Update()
    {
        if (!timerRunning) return;
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        timeLeft -= dt;
        if (timeLeft < 0f) timeLeft = 0f;
        UpdateTimerDisplay();
        if (timeLeft <= 0f)
        {
            EndLevel();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        int t = Mathf.CeilToInt(timeLeft);
        timerText.text = t.ToString();
    }
}
