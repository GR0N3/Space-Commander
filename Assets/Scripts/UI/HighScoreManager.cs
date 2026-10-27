using System;
using System.Collections.Generic;
using UnityEngine;

public class HighScoreManager : MonoBehaviour
{
    [Serializable]
    public class Entry : IComparable<Entry>
    {
        public string initials;
        public int score;

        public int CompareTo(Entry other)
        {
            if (other == null) return 1;
            // Orden descendente por puntaje (mayor a menor)
            return other.score.CompareTo(this.score);
        }
    }

    private static HighScoreManager instance;
    public static HighScoreManager Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("HighScoreManager");
                instance = go.AddComponent<HighScoreManager>();
                DontDestroyOnLoad(go);
                instance.Load();
            }
            return instance;
        }
    }

    [Header("Top Scores Data Structure")]
    [SerializeField] private int capacity = 10;
    private TopScoresCollection topScores = new TopScoresCollection(10);

    // Evento para el patrón Observer
    public static event Action OnScoresChanged;

    private const string PrefKey = "HighScores_v1";
    private const string PrefLastKey = "HighScores_LastInitials";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public TopScoresCollection GetTopScores() => topScores;

    public IReadOnlyList<Entry> GetEntries() => topScores != null ? topScores.ToList() : new List<Entry>();

    public int Add(string initials, int score)
    {
        if (string.IsNullOrEmpty(initials)) initials = "AAA";
        initials = initials.ToUpperInvariant();
        if (initials.Length > 3) initials = initials.Substring(0, 3);
        if (initials.Length < 3) initials = initials.PadRight(3, ' ');

        if (topScores == null) topScores = new TopScoresCollection(capacity);

        int rank = topScores.Add(initials, score);
        SetLastInitials(initials.Trim());
        Save();

        OnScoresChanged?.Invoke();
        return rank;
    }

    public void Clear()
    {
        if (topScores != null) topScores.Clear();
        Save();
        OnScoresChanged?.Invoke();
    }

    public void Load()
    {
        if (topScores == null) topScores = new TopScoresCollection(capacity);
        try
        {
            var json = PlayerPrefs.GetString(PrefKey, "");
            if (!string.IsNullOrEmpty(json))
            {
                var wrapper = JsonUtility.FromJson<Wrapper>(json);
                if (wrapper != null && wrapper.items != null)
                {
                    topScores.Clear();
                    foreach (var item in wrapper.items)
                    {
                        topScores.Add(item);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[HighScoreManager] Error al cargar puntajes: " + ex.Message);
            topScores = new TopScoresCollection(capacity);
        }
    }

    public void Save()
    {
        if (topScores == null) return;
        var wrapper = new Wrapper { items = topScores.ToArray() };
        var json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(PrefKey, json);
        PlayerPrefs.Save();
    }

    public string GetLastInitials()
    {
        return PlayerPrefs.GetString(PrefLastKey, "");
    }

    public void SetLastInitials(string initials)
    {
        if (string.IsNullOrEmpty(initials)) initials = "";
        initials = initials.ToUpperInvariant();
        if (initials.Length > 3) initials = initials.Substring(0, 3);
        PlayerPrefs.SetString(PrefLastKey, initials);
        PlayerPrefs.Save();
    }

    [Serializable]
    private class Wrapper { public Entry[] items; }
}
