using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;
    [SerializeField] private bool resetScoreOnQuit = true;

    private bool isPaused;
    private float prevTimeScale = 1f;

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    void Update()
    {
        if (isPaused)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) QuitToMenu();
            else if (Input.anyKeyDown) Resume();
        }
        else if (Input.GetKeyDown(toggleKey))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused) Resume(); else Pause();
    }

    private void Pause()
    {
        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        if (panel != null) panel.SetActive(true);
        isPaused = true;
    }

    public void Resume()
    {
        if (panel != null) panel.SetActive(false);
        AudioListener.pause = false;
        Time.timeScale = prevTimeScale <= 0f ? 1f : prevTimeScale;
        isPaused = false;
    }

    public void RestartLevel()
    {
        AudioListener.pause = false;
        Time.timeScale = 1f;
        SceneTransition.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMenu()
    {
        AudioListener.pause = false;
        Time.timeScale = 1f;
        if (resetScoreOnQuit && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }
        SceneTransition.LoadScene(menuSceneName);
    }
}
