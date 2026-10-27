using System.Text;
using UnityEngine;
using TMPro;

public class HighScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tableText;
    [SerializeField] private string lineFormat = "{0,2}. {1,-3}  {2,6}";
    [SerializeField] private bool showEmptySlots = true;
    [SerializeField] private int maxDisplayCount = 10;

    void Awake()
    {
        if (tableText == null)
        {
            tableText = GetComponent<TextMeshProUGUI>();
            if (tableText == null)
            {
                tableText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }
    }

    void OnEnable()
    {
        HighScoreManager.OnScoresChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        HighScoreManager.OnScoresChanged -= Refresh;
    }

    public void Refresh()
    {
        if (tableText == null)
        {
            tableText = GetComponent<TextMeshProUGUI>();
            if (tableText == null) return;
        }

        var mgr = HighScoreManager.Instance;
        var entries = mgr != null ? mgr.GetEntries() : null;

        var sb = new StringBuilder();
        int count = entries != null ? entries.Count : 0;

        for (int rank = 1; rank <= maxDisplayCount; rank++)
        {
            if (rank <= count)
            {
                var e = entries[rank - 1];
                sb.AppendLine(string.Format(lineFormat, rank, e.initials, e.score));
            }
            else if (showEmptySlots)
            {
                sb.AppendLine(string.Format(lineFormat, rank, "---", "0000"));
            }
        }

        tableText.text = sb.ToString();
    }
}
