using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image contentImage;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    private void OnCloseButtonClicked()
    {
        Destroy(gameObject);
    }

    public void SetMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    public void SetImage(Sprite sprite)
    {
        if (contentImage != null)
        {
            contentImage.sprite = sprite;
            // Optional: Preserve aspect ratio if needed
            // contentImage.preserveAspect = true; 
        }
    }
}
