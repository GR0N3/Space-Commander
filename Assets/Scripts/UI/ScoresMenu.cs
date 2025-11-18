using UnityEngine;

public class ScoresMenu : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private bool resetCurrentScoreOnQuit = true;

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        var initials = Object.FindFirstObjectByType<InitialsInput>();
        if (initials != null) initials.SubmitIfHasInitials();
        if (resetCurrentScoreOnQuit && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }
        SceneTransition.LoadScene(menuSceneName);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitToMenu();
        }
    }
}
