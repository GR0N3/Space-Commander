using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public interface IScoreService
{
    void AddScore(int points);
    int Score { get; }
}

public class ScoreManager : MonoBehaviour, IScoreService
{
    private static ScoreManager instance;
    [SerializeField] private int score;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private string format = "Points: {0}";

    public static ScoreManager Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("ScoreManager");
                instance = go.AddComponent<ScoreManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    public int Score => score;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryAutoBindUI();
        UpdateText();
    }

    private void TryAutoBindUI()
    {
        if (scoreText != null) return;
        var go = GameObject.Find("ScoreCount");
        if (go == null) go = GameObject.Find("Score");
        if (go != null)
        {
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp != null) scoreText = tmp;
        }
    }

    private void UpdateText()
    {
        if (scoreText != null) scoreText.text = string.Format(format, score);
    }

    public void BindUI(TextMeshProUGUI text)
    {
        scoreText = text;
        UpdateText();
    }

    public void AddScore(int points)
    {
        if (points <= 0) return;
        score += points;
        UpdateText();
    }

    public void ResetScore()
    {
        score = 0;
        UpdateText();
    }
}
