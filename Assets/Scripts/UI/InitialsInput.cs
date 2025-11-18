using UnityEngine;
using TMPro;

public class InitialsInput : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI initialsText;
    [SerializeField] private int maxChars = 3;
    [SerializeField] private string placeholder = "___";

    private string current = "";

    void OnEnable()
    {
        UpdateText();
    }

    void Update()
    {
        foreach (char c in Input.inputString)
        {
            if (c == '\b')
            {
                if (current.Length > 0) current = current.Substring(0, current.Length - 1);
            }
            else if (c == '\n' || c == '\r')
            {
                Submit();
            }
            else
            {
                if (char.IsLetterOrDigit(c) && current.Length < maxChars)
                {
                    current += char.ToUpperInvariant(c);
                }
            }
        }
        UpdateText();
    }

    private void UpdateText()
    {
        if (initialsText != null)
        {
            var display = current.PadRight(maxChars, '_');
            initialsText.text = display;
        }
    }

    public void Submit()
    {
        var score = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
        HighScoreManager.Instance.Add(string.IsNullOrEmpty(current) ? "AAA" : current, score);
        current = "";
        UpdateText();
    }

    public bool HasPending()
    {
        return !string.IsNullOrEmpty(current);
    }

    public void SubmitIfHasInitials()
    {
        if (HasPending()) Submit();
    }
}
