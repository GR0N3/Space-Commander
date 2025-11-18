using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string scoreboardSceneName = "Scores";
    [SerializeField] private int scoreboardSceneIndex = -1;

    public void StartGame()
    {
        Time.timeScale = 1f;
        var _ = ScoreManager.Instance;
        SceneTransition.LoadScene(1);
    }

    public void OpenScoreboard()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(scoreboardSceneName))
        {
            SceneTransition.LoadScene(scoreboardSceneName);
            return;
        }
        if (scoreboardSceneIndex >= 0)
        {
            SceneTransition.LoadScene(scoreboardSceneIndex);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
