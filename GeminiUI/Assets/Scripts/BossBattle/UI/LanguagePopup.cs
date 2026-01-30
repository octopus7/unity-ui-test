using UnityEngine;
using UnityEngine.UI;

public class LanguagePopup : MonoBehaviour
{
    public Button koreanBtn;
    public Button englishBtn;
    public Button japaneseBtn;
    public Button closeBtn;

    private void Start()
    {
        koreanBtn.onClick.AddListener(() => SetLanguage(GameLanguage.Korean));
        englishBtn.onClick.AddListener(() => SetLanguage(GameLanguage.English));
        japaneseBtn.onClick.AddListener(() => SetLanguage(GameLanguage.Japanese));
        
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(Close);
        }
    }

    private void SetLanguage(GameLanguage lang)
    {
        LocalizationManager.Instance.SetLanguage(lang);
        Close();
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }
}
