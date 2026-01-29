using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class PopupControllerCreator
{
    [MenuItem("Tools/Create Popup Controller Prefab")]
    public static void CreatePopupControllerPrefab()
    {
        // 1. Root and Canvas
        GameObject rootGO = new GameObject("PopupTester");
        Canvas canvas = rootGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        rootGO.AddComponent<CanvasScaler>();
        rootGO.AddComponent<GraphicRaycaster>();

        PopupController controller = rootGO.AddComponent<PopupController>();

        // 2. Load TestPopup Prefab
        string popupPrefabPath = "Assets/Prefabs/TestPopup.prefab";
        TestPopup popupPrefab = AssetDatabase.LoadAssetAtPath<TestPopup>(popupPrefabPath);
        
        if (popupPrefab == null)
        {
            Debug.LogError($"Could not find TestPopup prefab at {popupPrefabPath}. Please ensure it exists.");
            // We continue anyway, user can assign it manually
        }

        // 3. Create Buttons
        GameObject btn1GO = CreateButton("Button1", "Show Message 1", new Vector2(0, 100), rootGO.transform);
        GameObject btn2GO = CreateButton("Button2", "Show Message 2", new Vector2(0, 0), rootGO.transform);
        GameObject btn3GO = CreateButton("Button3", "Show Message 3", new Vector2(0, -100), rootGO.transform);

        Button btn1 = btn1GO.GetComponent<Button>();
        Button btn2 = btn2GO.GetComponent<Button>();
        Button btn3 = btn3GO.GetComponent<Button>();

        // 4. Bind References
        // Using Reflection to assign private fields for setup
        SetPrivateField(controller, "popupPrefab", popupPrefab);
        SetPrivateField(controller, "mainCanvas", canvas);
        SetPrivateField(controller, "button1", btn1);
        SetPrivateField(controller, "button2", btn2);
        SetPrivateField(controller, "button3", btn3);

        // 5. Save Prefab
        string folderPath = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        string prefabPath = "Assets/Prefabs/PopupControllerUI.prefab";
        PrefabUtility.SaveAsPrefabAsset(rootGO, prefabPath);

        // 6. Cleanup
        Object.DestroyImmediate(rootGO);

        Debug.Log($"PopupController Prefab created at {prefabPath}");
    }

    private static GameObject CreateButton(string name, string label, Vector2 anchoredPosition, Transform parent)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        
        Image img = btnGO.AddComponent<Image>();
        img.color = Color.white;
        
        Button btn = btnGO.AddComponent<Button>();
        
        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(160, 50);
        rt.anchoredPosition = anchoredPosition;

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        
        // Try to use TMP if possible, falling back to legacy Text if TMP essential resources missing (unlikely in modern Unity)
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 24;
        
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        return btnGO;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            Debug.LogWarning($"Field {fieldName} not found on {target.GetType().Name}");
        }
    }
}
