using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultPopup : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Button closeButton;

    public void Setup(string title, string message)
    {
        titleText.text = title;
        messageText.text = message;
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }
}

public class DamagePopup : MonoBehaviour
{
    public TextMeshProUGUI damageText;

    public void Setup(int damage)
    {
        damageText.text = $"-{damage}";
        // Simple animation or auto-destroy
        StartCoroutine(AnimateAndDestroy());
    }

    private IEnumerator AnimateAndDestroy()
    {
        float duration = 1.0f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * 50f;
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            group.alpha = 1f - t;
            yield return null;
        }
        Destroy(gameObject);
    }
}
