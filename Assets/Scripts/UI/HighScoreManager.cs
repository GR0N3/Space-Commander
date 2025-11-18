using System;
using System.Collections.Generic;
using UnityEngine;

public class HighScoreManager : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public string initials;
        public int score;
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

    [SerializeField] private int capacity = 10;
    [SerializeField] private List<Entry> entries = new List<Entry>();
    private const string PrefKey = "HighScores_v1";

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

    public IReadOnlyList<Entry> GetEntries() => entries;

    public void Add(string initials, int score)
    {
        if (string.IsNullOrEmpty(initials)) initials = "AAA";
        initials = initials.ToUpperInvariant();
        if (initials.Length > 3) initials = initials.Substring(0, 3);
        if (initials.Length < 3) initials = initials.PadRight(3, ' ');

        entries.Add(new Entry { initials = initials, score = score });
        entries.Sort((a, b) => b.score.CompareTo(a.score));
        if (entries.Count > capacity) entries.RemoveRange(capacity, entries.Count - capacity);
        Save();
    }

    public void Clear()
    {
        entries.Clear();
        Save();
    }

    private void Load()
    {
        try
        {
            var json = PlayerPrefs.GetString(PrefKey, "");
            if (!string.IsNullOrEmpty(json))
            {
                var wrapper = JsonUtility.FromJson<Wrapper>(json);
                entries = wrapper != null && wrapper.items != null ? new List<Entry>(wrapper.items) : new List<Entry>();
            }
        }
        catch { entries = new List<Entry>(); }
    }

    private void Save()
    {
        var wrapper = new Wrapper { items = entries.ToArray() };
        var json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(PrefKey, json);
        PlayerPrefs.Save();
    }

    [Serializable]
    private class Wrapper { public Entry[] items; }
}
