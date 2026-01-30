using UnityEngine;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    public string key;
    private TextMeshProUGUI _tmpText;

    private void Awake()
    {
        _tmpText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        LocalizationManager.Instance.OnLanguageChanged += UpdateText;
        UpdateText();
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
        }
    }

    private void UpdateText()
    {
        if (_tmpText != null)
        {
            // 1. Update Font
            // This ensures CJK rendering changes based on language preference (e.g. Hanja vs Kanji)
            var font = LocalizationManager.Instance.GetFont(LocalizationManager.Instance.CurrentLanguage);
            if (font != null)
            {
                _tmpText.font = font;
            }

            // 2. Update Text
            if (!string.IsNullOrEmpty(key))
            {
                _tmpText.text = LocalizationManager.Instance.GetString(key);
            }
        }
    }

    // Helper for editor setting
    public void SetKey(string newKey)
    {
        key = newKey;
        UpdateText();
    }
}
