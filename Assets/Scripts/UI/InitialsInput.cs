using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class InitialsInput : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI initialsText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private int maxChars = 3;
    [SerializeField] private string placeholder = "___";
    [SerializeField] private bool clearOnSubmit = false;
    [SerializeField] private bool syncPreviewWithInputArea = true;

    private RectTransform inputTextRect;
    private RectTransform previewRect;

    private string current = "";
    private bool submitted;

    void OnEnable()
    {
        if (EventSystem.current == null)
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
            DontDestroyOnLoad(go);
        }
        if (inputField == null)
        {
            inputField = GetComponentInChildren<TMP_InputField>(true);
            if (inputField == null)
            {
                inputField = Object.FindFirstObjectByType<TMP_InputField>();
            }
        }
        if (inputField != null)
        {
            inputField.characterLimit = maxChars;
            inputField.characterValidation = TMP_InputField.CharacterValidation.Alphanumeric;
            inputField.contentType = TMP_InputField.ContentType.Alphanumeric;
            inputField.lineType = TMP_InputField.LineType.SingleLine;
            inputField.onValueChanged.AddListener(OnInputChanged);
            inputField.ActivateInputField();
            inputField.Select();
        }
        string last = HighScoreManager.Instance != null ? HighScoreManager.Instance.GetLastInitials() : "";
        if (!string.IsNullOrEmpty(last))
        {
            current = last;
            if (inputField != null) inputField.SetTextWithoutNotify(last);
        }
        inputTextRect = inputField != null && inputField.textComponent != null ? inputField.textComponent.rectTransform : null;
        previewRect = initialsText != null ? initialsText.rectTransform : null;
        if (syncPreviewWithInputArea) SyncPreviewRect();
        UpdateText();
    }

    void OnDisable()
    {
        if (!submitted && HasPending()) Submit();
    }

    void Update()
    {
        if (inputField == null)
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
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Submit();
            }
        }
        if (syncPreviewWithInputArea) SyncPreviewRect();
        UpdateText();
    }

    private void UpdateText()
    {
        if (initialsText != null)
        {
            string src = inputField != null ? inputField.text : current;
            src = src.ToUpperInvariant();
            if (src.Length > maxChars) src = src.Substring(0, maxChars);
            var display = src.PadRight(maxChars, '_');
            initialsText.text = display;
        }
    }

    public void Submit()
    {
        var score = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
        string initials = inputField != null ? inputField.text : current;
        HighScoreManager.Instance.Add(string.IsNullOrEmpty(initials) ? "AAA" : initials, score);
        submitted = true;
        if (clearOnSubmit)
        {
            current = "";
            if (inputField != null) inputField.text = "";
            UpdateText();
        }
    }

    public bool HasPending()
    {
        string initials = inputField != null ? inputField.text : current;
        return !string.IsNullOrEmpty(initials);
    }

    private void SyncPreviewRect()
    {
        if (inputTextRect == null || previewRect == null) return;
        previewRect.anchorMin = inputTextRect.anchorMin;
        previewRect.anchorMax = inputTextRect.anchorMax;
        previewRect.pivot = inputTextRect.pivot;
        previewRect.anchoredPosition = inputTextRect.anchoredPosition;
        previewRect.sizeDelta = inputTextRect.sizeDelta;
    }

    private void OnInputChanged(string value)
    {
        string u = string.IsNullOrEmpty(value) ? string.Empty : value.ToUpperInvariant();
        if (u.Length > maxChars) u = u.Substring(0, maxChars);
        if (inputField != null && inputField.text != u)
        {
            inputField.SetTextWithoutNotify(u);
        }
        UpdateText();
    }

    public void SubmitIfHasInitials()
    {
        if (HasPending()) Submit();
    }
}
