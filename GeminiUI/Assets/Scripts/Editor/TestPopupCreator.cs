using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class TestPopupCreator
{
    [MenuItem("Tools/Create Test Popup Prefab")]
    public static void CreateTestPopupPrefab()
    {
        // 1. Root Object (Just a RectTransform, NO Canvas/CanvasScaler)
        // Having CanvasScaler on a child object inside another Canvas causes layout issues.
        GameObject rootGO = new GameObject("TestPopup");
        rootGO.AddComponent<RectTransform>();
        
        TestPopup testPopupScript = rootGO.AddComponent<TestPopup>();

        // 2. Background Dim
        GameObject dimGO = new GameObject("BackgroundDim");
        dimGO.transform.SetParent(rootGO.transform, false);
        Image dimImage = dimGO.AddComponent<Image>();
        dimImage.color = new Color(0, 0, 0, 0.5f); // Semi-transparent black
        RectTransform dimRect = dimImage.rectTransform;
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.sizeDelta = Vector2.zero; // Stretch to fill

        // 3. Popup Panel
        GameObject panelGO = new GameObject("PopupPanel");
        panelGO.transform.SetParent(rootGO.transform, false);
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = Color.white;
        RectTransform panelRect = panelImage.rectTransform;
        panelRect.sizeDelta = new Vector2(400, 300); // 400x300 size

        // 4. Content Image
        GameObject imageGO = new GameObject("ContentImage");
        imageGO.transform.SetParent(panelGO.transform, false);
        Image contentImage = imageGO.AddComponent<Image>();
        contentImage.color = Color.gray; // Placeholder color
        RectTransform imageRect = contentImage.rectTransform;
        imageRect.anchorMin = new Vector2(0.5f, 1f);
        imageRect.anchorMax = new Vector2(0.5f, 1f);
        imageRect.pivot = new Vector2(0.5f, 1f);
        imageRect.anchoredPosition = new Vector2(0, -20);
        imageRect.sizeDelta = new Vector2(300, 150);
        
        // 5. Message Text
        GameObject textGO = new GameObject("MessageText");
        textGO.transform.SetParent(panelGO.transform, false);
        TextMeshProUGUI messageText = textGO.AddComponent<TextMeshProUGUI>();
        messageText.text = "This is a test message.";
        messageText.color = Color.black;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.fontSize = 20;
        RectTransform textRect = messageText.rectTransform;
        textRect.anchorMin = new Vector2(0.5f, 0.5f); // Center
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(0, -10); // Below image
        textRect.sizeDelta = new Vector2(360, 50);

        // 6. Close Button
        GameObject btnGO = new GameObject("CloseButton");
        btnGO.transform.SetParent(panelGO.transform, false);
        Image btnImage = btnGO.AddComponent<Image>();
        btnImage.color = new Color(0.9f, 0.9f, 0.9f);
        Button closeBtn = btnGO.AddComponent<Button>();
        RectTransform btnRect = btnImage.rectTransform;
        btnRect.anchorMin = new Vector2(0.5f, 0f); // Bottom center
        btnRect.anchorMax = new Vector2(0.5f, 0f);
        btnRect.pivot = new Vector2(0.5f, 0f);
        btnRect.anchoredPosition = new Vector2(0, 20);
        btnRect.sizeDelta = new Vector2(100, 40);

        // Button Text
        GameObject btnTextGO = new GameObject("Text");
        btnTextGO.transform.SetParent(btnGO.transform, false);
        TextMeshProUGUI btnText = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnText.text = "Close";
        btnText.color = Color.black;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.fontSize = 18;
        RectTransform btnTextRect = btnText.rectTransform;
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;

        // 7. Bind Script References using SerializedObject to avoid "dirty" issues if needed, 
        // but direct assignment usually works fine for new objects before prefab saving.
        // However, we are in Editor code.
        testPopupScript.GetType().GetField("messageText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(testPopupScript, messageText);
        testPopupScript.GetType().GetField("contentImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(testPopupScript, contentImage);
        testPopupScript.GetType().GetField("closeButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(testPopupScript, closeBtn);

        // 8. Create Prefab
        string folderPath = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        string prefabPath = "Assets/Prefabs/TestPopup.prefab";
        PrefabUtility.SaveAsPrefabAsset(rootGO, prefabPath);

        // 9. Cleanup
        Object.DestroyImmediate(rootGO);

        Debug.Log($"TestPopup Prefab created at {prefabPath}");
    }
}
