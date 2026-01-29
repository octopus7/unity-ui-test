using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

[InitializeOnLoad]
public class BossBattleUI_Gen
{
    static BossBattleUI_Gen()
    {
        EditorApplication.delayCall += GenerateAll;
    }

    [MenuItem("GeminiUI/Generate All BossBattle UI")]
    public static void ForceGenerateAll()
    {
        // Reset flag to force regeneration
        SessionState.SetBool("BossBattleUI_Gen_Done_V2", false);
        GenerateAll();
    }

    private static void GenerateAll()
    {
        if (SessionState.GetBool("BossBattleUI_Gen_Done_V2", false)) return;
        SessionState.SetBool("BossBattleUI_Gen_Done_V2", true);

        if (!Directory.Exists("Assets/Prefabs/Battle"))
        {
            Directory.CreateDirectory("Assets/Prefabs/Battle");
        }

        GameObject itemPrefabObj = null;
        GameObject resultPopupObj = null;
        GameObject damagePopupObj = null;
        GameObject loginPopupObj = null;

        try
        {
            // 1. Create Items & Popups first
            itemPrefabObj = GenerateBattleListItem();
            resultPopupObj = GenerateResultPopup();
            damagePopupObj = GenerateDamagePopup();
            loginPopupObj = GenerateLoginPopup();

            // Refresh to ensure LoadAssetAtPath finds them
            AssetDatabase.Refresh();

            // 2. Create Lobby View (Depends on Item)
            GenerateLobbyView(itemPrefabObj, resultPopupObj, damagePopupObj, loginPopupObj);

            Debug.Log("[BossBattleUI_Gen] All Prefabs Generated in Assets/Prefabs/Battle/");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[BossBattleUI_Gen] Generation Failed: {e}");
        }
        finally
        {
            // Cleanup
            if (itemPrefabObj) Object.DestroyImmediate(itemPrefabObj);
            if (resultPopupObj) Object.DestroyImmediate(resultPopupObj);
            if (damagePopupObj) Object.DestroyImmediate(damagePopupObj);
            if (loginPopupObj) Object.DestroyImmediate(loginPopupObj);
        }
    }

    // --- Helper for RectTransform ---
    private static void SetFullStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    // 1. BattleListItem
    private static GameObject GenerateBattleListItem()
    {
        GameObject root = new GameObject("BattleListItem");
        root.AddComponent<RectTransform>();
        
        // Add LayoutElement for Auto Layout
        var le = root.AddComponent<LayoutElement>();
        le.preferredHeight = 80;
        le.minHeight = 80;

        var script = root.AddComponent<BattleListItem>();
        var img = root.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f); // Dark BG

        // Layout
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 80); // Height 80

        // MyBattle Icon
        var iconObj = new GameObject("MyTag");
        iconObj.transform.SetParent(root.transform, false);
        var iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 1);
        iconRect.anchorMax = new Vector2(0, 1);
        iconRect.pivot = new Vector2(0, 1);
        iconRect.anchoredPosition = new Vector2(5, -5);
        iconRect.sizeDelta = new Vector2(30, 30);
        var iconImg = iconObj.AddComponent<Image>();
        iconImg.color = Color.cyan;
        script.myBattleTag = iconImg;

        // Host Name
        var hostObj = new GameObject("HostName");
        hostObj.transform.SetParent(root.transform, false);
        var hostRect = hostObj.AddComponent<RectTransform>();
        hostRect.anchoredPosition = new Vector2(50, 15);
        hostRect.sizeDelta = new Vector2(300, 30); // Explicit Size
        hostRect.anchorMin = new Vector2(0, 0.5f); // Left align relative to parent
        hostRect.anchorMax = new Vector2(0, 0.5f);
        hostRect.pivot = new Vector2(0, 0.5f);
        
        var hostText = hostObj.AddComponent<TextMeshProUGUI>();
        hostText.text = "Host: User";
        hostText.fontSize = 20;
        hostText.color = Color.white;
        hostText.alignment = TextAlignmentOptions.Left;
        hostText.enableWordWrapping = false;
        script.hostNameText = hostText;

        // HP Slider
        var sliderObj = new GameObject("HPSlider");
        sliderObj.transform.SetParent(root.transform, false);
        var sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(300, 20);
        sliderRect.anchoredPosition = new Vector2(50, -15);
        sliderRect.anchorMin = new Vector2(0, 0.5f);
        sliderRect.anchorMax = new Vector2(0, 0.5f);
        sliderRect.pivot = new Vector2(0, 0.5f);
        var slider = sliderObj.AddComponent<Slider>();
        
        // Slider BG & Fill (Simplified)
        var slBg = new GameObject("BG");
        slBg.transform.SetParent(sliderObj.transform, false);
        slBg.AddComponent<Image>().color = Color.gray;
        SetFullStretch(slBg.GetComponent<RectTransform>()); 
        
        var slFillArea = new GameObject("Fill Area");
        slFillArea.transform.SetParent(sliderObj.transform, false);
        var tr = slFillArea.AddComponent<RectTransform>();
        SetFullStretch(tr);
        
        var slFill = new GameObject("Fill");
        slFill.transform.SetParent(slFillArea.transform, false);
        var fillImg = slFill.AddComponent<Image>();
        fillImg.color = Color.red;
        SetFullStretch(slFill.GetComponent<RectTransform>()); 

        slider.targetGraphic = slBg.GetComponent<Image>();
        slider.fillRect = slFill.GetComponent<RectTransform>();
        script.hpSlider = slider;

        // HP Text
        var hpTextObj = new GameObject("HPText");
        hpTextObj.transform.SetParent(sliderObj.transform, false);
        var hpText = hpTextObj.AddComponent<TextMeshProUGUI>();
        hpText.text = "500/1000";
        hpText.fontSize = 14;
        hpText.alignment = TextAlignmentOptions.Center;
        SetFullStretch(hpText.rectTransform);
        script.hpText = hpText;

        // Participate Button
        var btnObj = new GameObject("ParticipateBtn");
        btnObj.transform.SetParent(root.transform, false);
        var btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 0.5f);
        btnRect.anchorMax = new Vector2(1, 0.5f);
        btnRect.pivot = new Vector2(1, 0.5f);
        btnRect.anchoredPosition = new Vector2(-10, 0);
        btnRect.sizeDelta = new Vector2(100, 50);
        btnObj.AddComponent<Image>().color = Color.green;
        var btn = btnObj.AddComponent<Button>();
        script.participateButton = btn;

        var btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        var btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "Participate";
        btnText.fontSize = 16;
        btnText.alignment = TextAlignmentOptions.Center;
        SetFullStretch(btnText.rectTransform);

        // Status Text
        var statusObj = new GameObject("Status");
        statusObj.transform.SetParent(root.transform, false);
        var statusText = statusObj.AddComponent<TextMeshProUGUI>();
        statusText.text = "Time: ...";
        statusText.fontSize = 14;
        statusText.color = Color.yellow;
        statusText.rectTransform.anchorMin = new Vector2(1, 1);
        statusText.rectTransform.anchorMax = new Vector2(1, 1);
        statusText.rectTransform.pivot = new Vector2(1, 1);
        statusText.rectTransform.anchoredPosition = new Vector2(-120, -10);
        statusText.rectTransform.sizeDelta = new Vector2(150, 20); // Explicit Size
        script.statusText = statusText;

        // SET UI LAYER
        SetLayerRecursively(root, LayerMask.NameToLayer("UI"));

        string path = "Assets/Prefabs/Battle/BattleListItem.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        return root;
    }

    // 2. ResultPopup
    private static GameObject GenerateResultPopup()
    {
        GameObject root = new GameObject("ResultPopup");
        var rt = root.AddComponent<RectTransform>();
        SetFullStretch(rt);
        var script = root.AddComponent<ResultPopup>();

        // BG Dim
        var bg = root.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);

        // Panel
        var panel = new GameObject("Panel");
        panel.transform.SetParent(root.transform, false);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = Color.white;
        panel.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 300);

        // Title
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        var titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
        titleTxt.text = "TITLE";
        titleTxt.fontSize = 32;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.color = Color.black;
        titleTxt.rectTransform.anchoredPosition = new Vector2(0, 100);
        titleTxt.rectTransform.sizeDelta = new Vector2(300, 50); // Explicit
        script.titleText = titleTxt;

        // Message
        var msgObj = new GameObject("Message");
        msgObj.transform.SetParent(panel.transform, false);
        var msgTxt = msgObj.AddComponent<TextMeshProUGUI>();
        msgTxt.text = "Message...";
        msgTxt.fontSize = 20;
        msgTxt.alignment = TextAlignmentOptions.Center;
        msgTxt.color = Color.black;
        msgTxt.rectTransform.sizeDelta = new Vector2(350, 100); // Explicit
        script.messageText = msgTxt;

        // Close Button
        var btnObj = new GameObject("CloseBtn");
        btnObj.transform.SetParent(panel.transform, false);
        var btnImg = btnObj.AddComponent<Image>(); // Adds RectTransform
        btnImg.color = Color.gray;
        
        var btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchoredPosition = new Vector2(0, -100);
        btnRect.sizeDelta = new Vector2(150, 40);
        
        var btn = btnObj.AddComponent<Button>();
        script.closeButton = btn;

        var btnTxtObj = new GameObject("Text");
        btnTxtObj.transform.SetParent(btnObj.transform, false);
        var btnTxt = btnTxtObj.AddComponent<TextMeshProUGUI>();
        btnTxt.text = "Close";
        btnTxt.alignment = TextAlignmentOptions.Center;
        btnTxt.color = Color.black;
        SetFullStretch(btnTxt.rectTransform);

        SetLayerRecursively(root, LayerMask.NameToLayer("UI"));
        
        string path = "Assets/Prefabs/Battle/ResultPopup.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        return root;
    }

    // 3. DamagePopup
    private static GameObject GenerateDamagePopup()
    {
        GameObject root = new GameObject("DamagePopup");
        root.AddComponent<RectTransform>();
        var script = root.AddComponent<DamagePopup>();

        var txtObj = new GameObject("Text");
        txtObj.transform.SetParent(root.transform, false);
        var txt = txtObj.AddComponent<TextMeshProUGUI>();
        txt.text = "-100";
        txt.fontSize = 36;
        txt.color = Color.red;
        txt.alignment = TextAlignmentOptions.Center;
        txt.rectTransform.sizeDelta = new Vector2(200, 50);
        script.damageText = txt;

        SetLayerRecursively(root, LayerMask.NameToLayer("UI"));

        string path = "Assets/Prefabs/Battle/DamagePopup.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        return root;
    }

    // 4. LoginPopup
    private static GameObject GenerateLoginPopup()
    {
        GameObject root = new GameObject("LoginPopup");
        var rt = root.AddComponent<RectTransform>();
        SetFullStretch(rt);
        var script = root.AddComponent<LoginPopup>();

        // BG
        root.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.3f);

        // Input Field
        var inputObj = new GameObject("InputID");
        inputObj.transform.SetParent(root.transform, false);
        var inputImg = inputObj.AddComponent<Image>();
        inputImg.color = Color.white;
        var input = inputObj.AddComponent<TMP_InputField>();
        inputObj.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 50);
        
        var textArea = new GameObject("TextArea");
        textArea.transform.SetParent(inputObj.transform, false);
        SetFullStretch(textArea.AddComponent<RectTransform>());
        var text = textArea.AddComponent<TextMeshProUGUI>();
        text.color = Color.black;
        input.textComponent = text;
        input.textViewport = textArea.GetComponent<RectTransform>();
        script.userIdInput = input;

        // Button
        var btnObj = new GameObject("StartBtn");
        btnObj.transform.SetParent(root.transform, false);
        
        var btnImg = btnObj.AddComponent<Image>(); // Adds RectTransform
        btnImg.color = Color.green;

        var btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchoredPosition = new Vector2(0, -80);
        btnRect.sizeDelta = new Vector2(200, 50);

        var btn = btnObj.AddComponent<Button>();
        script.startButton = btn;

        var btnTxtObj = new GameObject("Text");
        btnTxtObj.transform.SetParent(btnObj.transform, false);
        var btnTxt = btnTxtObj.AddComponent<TextMeshProUGUI>();
        btnTxt.text = "GAME START";
        btnTxt.fontSize = 24;
        btnTxt.alignment = TextAlignmentOptions.Center;
        SetFullStretch(btnTxt.rectTransform);

        SetLayerRecursively(root, LayerMask.NameToLayer("UI"));

        string path = "Assets/Prefabs/Battle/LoginPopup.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        return root;
    }

    // 5. LobbyView (Main)
    private static void GenerateLobbyView(GameObject itemPrefab, GameObject resPopup, GameObject dmgPopup, GameObject loginPopup)
    {
        GameObject root = new GameObject("LobbyView");
        var rt = root.AddComponent<RectTransform>();
        SetFullStretch(rt);
        var script = root.AddComponent<LobbyUI>();
        // Add Manager
        var manager = root.AddComponent<BattleResultManager>();
        manager.popupParent = root.transform; // Popups appear here

        // Load Prefabs for binding
         script.listItemPrefab = AssetDatabase.LoadAssetAtPath<BattleListItem>("Assets/Prefabs/Battle/BattleListItem.prefab");
         manager.resultPopupPrefab = AssetDatabase.LoadAssetAtPath<ResultPopup>("Assets/Prefabs/Battle/ResultPopup.prefab");
         manager.damagePopupPrefab = AssetDatabase.LoadAssetAtPath<DamagePopup>("Assets/Prefabs/Battle/DamagePopup.prefab");

        // UI Setup
        root.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f); // Dark Navy BG

        // Top Panel
        var topPanel = new GameObject("TopPanel");
        topPanel.transform.SetParent(root.transform, false);
        var topRect = topPanel.AddComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0, 1);
        topRect.anchorMax = new Vector2(1, 1);
        topRect.pivot = new Vector2(0.5f, 1);
        topRect.sizeDelta = new Vector2(0, 60);
        topRect.anchoredPosition = Vector2.zero;
        topPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);

        // Gold Text
        var goldObj = new GameObject("GoldText");
        goldObj.transform.SetParent(topPanel.transform, false);
        var goldTxt = goldObj.AddComponent<TextMeshProUGUI>();
        goldTxt.text = "Gold: 0";
        goldTxt.color = Color.yellow;
        goldTxt.fontSize = 24;
        goldTxt.rectTransform.anchorMin = new Vector2(0, 0.5f);
        goldTxt.rectTransform.anchorMax = new Vector2(0, 0.5f);
        goldTxt.rectTransform.pivot = new Vector2(0, 0.5f);
        goldTxt.rectTransform.anchoredPosition = new Vector2(20, 0);
        goldTxt.rectTransform.sizeDelta = new Vector2(200, 40); // Explicit
        script.goldText = goldTxt;

        // Refresh Button
        var refBtnObj = new GameObject("RefreshBtn");
        refBtnObj.transform.SetParent(topPanel.transform, false);
        var refRect = refBtnObj.AddComponent<RectTransform>();
        refRect.anchorMin = new Vector2(1, 0.5f);
        refRect.anchorMax = new Vector2(1, 0.5f);
        refRect.pivot = new Vector2(1, 0.5f);
        refRect.anchoredPosition = new Vector2(-250, 0); // Moved left to make room
        refRect.sizeDelta = new Vector2(160, 50); // Bigger (100x40 -> 160x50)
        refBtnObj.AddComponent<Image>().color = Color.blue;
        var refBtn = refBtnObj.AddComponent<Button>();
        script.refreshButton = refBtn;
        
        var refTxtObj = new GameObject("Text");
        refTxtObj.transform.SetParent(refBtnObj.transform, false);
        var refTxt = refTxtObj.AddComponent<TextMeshProUGUI>();
        refTxt.text = "Refresh";
        refTxt.alignment = TextAlignmentOptions.Center;
        SetFullStretch(refTxt.rectTransform);

         // Create Button
        var crtBtnObj = new GameObject("CreateBtn");
        crtBtnObj.transform.SetParent(topPanel.transform, false);
        var crtRect = crtBtnObj.AddComponent<RectTransform>();
        crtRect.anchorMin = new Vector2(1, 0.5f);
        crtRect.anchorMax = new Vector2(1, 0.5f);
        crtRect.pivot = new Vector2(1, 0.5f);
        crtRect.anchoredPosition = new Vector2(-20, 0);
        crtRect.sizeDelta = new Vector2(220, 50); // Bigger (150x40 -> 220x50)
        crtBtnObj.AddComponent<Image>().color = new Color(0.8f, 0.4f, 0);
        var crtBtn = crtBtnObj.AddComponent<Button>();
        script.createBattleButton = crtBtn;
        
        var crtTxtObj = new GameObject("Text");
        crtTxtObj.transform.SetParent(crtBtnObj.transform, false);
        var crtTxt = crtTxtObj.AddComponent<TextMeshProUGUI>();
        crtTxt.text = "Create Battle";
        crtTxt.alignment = TextAlignmentOptions.Center;
        SetFullStretch(crtTxt.rectTransform);

        // Scroll View
        var svObj = new GameObject("ScrollView");
        svObj.transform.SetParent(root.transform, false);
        var svRect = svObj.AddComponent<RectTransform>();
        SetFullStretch(svRect);
        svRect.offsetMin = new Vector2(20, 20);
        svRect.offsetMax = new Vector2(-20, -80); // Margin
        
        var scrollRect = svObj.AddComponent<ScrollRect>();
        svObj.AddComponent<Image>().color = new Color(0,0,0,0.3f);
        
        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(svObj.transform, false);
        var vpRect = viewport.AddComponent<RectTransform>();
        SetFullStretch(vpRect);
        
        // Use RectMask2D for better masking support with TMPro
        viewport.AddComponent<RectMask2D>();
        
        scrollRect.viewport = vpRect;

        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);
        
        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.childControlHeight = false; // Let items define their height (80)
        vlg.childForceExpandHeight = false; // Don't force them to fill empty space
        vlg.childControlWidth = true;
        vlg.spacing = 5;
        vlg.padding = new RectOffset(5, 5, 5, 5);
        content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRect;
        scrollRect.vertical = true;
        scrollRect.horizontal = false;
        script.listContent = contentRect;

        // Add Login Popup Instance (Hidden) so it can be controlled
        var loginInstance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Battle/LoginPopup.prefab"));
        loginInstance.transform.SetParent(root.transform, false);
        
        // Link Login to Lobby
        var loginScript = loginInstance.GetComponent<LoginPopup>();
        loginScript.lobbyUI = script;

        SetLayerRecursively(root, LayerMask.NameToLayer("UI"));

        PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Battle/LobbyView.prefab");
        Object.DestroyImmediate(root);
        Object.DestroyImmediate(loginInstance);
    }
}
