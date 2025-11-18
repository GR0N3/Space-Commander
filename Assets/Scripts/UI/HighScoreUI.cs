using System.Text;
using UnityEngine;
using TMPro;

public class HighScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tableText;
    [SerializeField] private string lineFormat = "{0}. {1} - {2}";

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        var mgr = HighScoreManager.Instance;
        var entries = mgr.GetEntries();
        if (tableText == null) return;
        var sb = new StringBuilder();
        int i = 1;
        foreach (var e in entries)
        {
            sb.AppendLine(string.Format(lineFormat, i, e.initials, e.score));
            i++;
        }
        tableText.text = sb.ToString();
    }
}
