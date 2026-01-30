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

    [MenuItem("GeminiUI/Generate All BossBattle UI", false, 20)]
    public static void ForceGenerateAll()
    {
        // Reset flag to force regeneration
        SessionState.SetBool("BossBattleUI_Gen_Done_V7", false);
        GenerateAll();
    }

    private static void GenerateAll()
    {
        if (SessionState.GetBool("BossBattleUI_Gen_Done_V7", false)) return;
        SessionState.SetBool("BossBattleUI_Gen_Done_V7", true);

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
            var langPopupObj = GenerateLanguagePopup(); // New
            loginPopupObj = GenerateLoginPopup();
            
            if (langPopupObj) Object.DestroyImmediate(langPopupObj); // Cleanup temp

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
        hostRect.sizeDelta = new Vector2(200, 30); // Reduced width to avoid overlap
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
        sliderRect.sizeDelta = new Vector2(200, 20); // Reduced width to avoid overlap
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
        var btnImg = btnObj.AddComponent<Image>();
        btnImg.color = Color.green;
        btnImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        btnImg.type = Image.Type.Sliced;
        btnImg.pixelsPerUnitMultiplier = 0.33f; // 3x Rounding
        var btn = btnObj.AddComponent<Button>();
        script.participateButton = btn;

        var btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        var btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "Participate";
        btnText.fontSize = 16;
        btnText.alignment = TextAlignmentOptions.Center;
        SetFullStretch(btnText.rectTransform);

        // Status Group Container
        var statusObj = new GameObject("StatusGroup");
        statusObj.transform.SetParent(root.transform, false);
        var statusRect = statusObj.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(1, 0.5f);
        statusRect.anchorMax = new Vector2(1, 0.5f);
        statusRect.pivot = new Vector2(1, 0.5f);
        statusRect.anchoredPosition = new Vector2(-120, 0); // Left of button
        statusRect.sizeDelta = new Vector2(250, 70);
        
        var hlg = statusObj.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlHeight = false;
        hlg.childControlWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleRight;

        // 1. Attempts Text (Left side of group)
        var attemptsObj = new GameObject("AttemptsText");
        attemptsObj.transform.SetParent(statusObj.transform, false);
        var attemptsTxt = attemptsObj.AddComponent<TextMeshProUGUI>();
        attemptsTxt.text = "<size=50%>Attempts</size>\n1/5";
        attemptsTxt.alignment = TextAlignmentOptions.Center; 
        attemptsTxt.fontSize = 28;
        attemptsTxt.color = Color.yellow;
        attemptsTxt.rectTransform.sizeDelta = new Vector2(80, 60);
        script.attemptsText = attemptsTxt;

        // 2. Time Badge (Background)
        var timeBadgeObj = new GameObject("TimeBadge");
        timeBadgeObj.transform.SetParent(statusObj.transform, false);
        var timeBadgeImage = timeBadgeObj.AddComponent<Image>();
        timeBadgeImage.color = new Color(0, 0, 0, 0.6f); // Dark Background
        // Try to load standard rounded background
        timeBadgeImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        timeBadgeImage.type = Image.Type.Sliced;
        timeBadgeImage.pixelsPerUnitMultiplier = 0.33f; // 3x Rounding effect (Lower multiplier = Bigger slices)
        
        var timeBadgeRect = timeBadgeObj.GetComponent<RectTransform>();
        timeBadgeRect.sizeDelta = new Vector2(140, 50); // Box Size
        
        // 3. Time Text (Inside Badge)
        var timeObj = new GameObject("TimeText");
        timeObj.transform.SetParent(timeBadgeObj.transform, false);
        var timeTxt = timeObj.AddComponent<TextMeshProUGUI>();
        timeTxt.text = "27m Left";
        timeTxt.alignment = TextAlignmentOptions.Center; // Center inside Badge
        timeTxt.fontSize = 24; // Slightly smaller to fit in box
        timeTxt.color = new Color(1f, 0.8f, 0.2f); // Gold-ish
        
        SetFullStretch(timeTxt.rectTransform); // Stretch to fill badge
        script.timeText = timeTxt;

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
        
        // LocalizedText
        var loc = btnTxtObj.AddComponent<LocalizedText>();
        loc.SetKey("UI_Login_GameStart");

        // Language Button (Bottom Right)
        var langBtnObj = new GameObject("LanguageBtn");
        langBtnObj.transform.SetParent(root.transform, false);
        var langBtnRect = langBtnObj.AddComponent<RectTransform>();
        langBtnRect.anchorMin = new Vector2(1, 0);
        langBtnRect.anchorMax = new Vector2(1, 0);
        langBtnRect.pivot = new Vector2(1, 0);
        langBtnRect.anchoredPosition = new Vector2(-20, 20);
        langBtnRect.sizeDelta = new Vector2(100, 40);
        
        var langBtnImg = langBtnObj.AddComponent<Image>();
        langBtnImg.color = Color.cyan;
        var langBtn = langBtnObj.AddComponent<Button>();
        
        var langTxtObj = new GameObject("Text");
        langTxtObj.transform.SetParent(langBtnObj.transform, false);
        var langTxt = langTxtObj.AddComponent<TextMeshProUGUI>();
        langTxt.text = "Language";
        langTxt.fontSize = 18;
        langTxt.color = Color.black;
        langTxt.alignment = TextAlignmentOptions.Center;
        SetFullStretch(langTxt.rectTransform);
        
        // LocalizedText
        var locLang = langTxtObj.AddComponent<LocalizedText>();
        locLang.SetKey("UI_Login_Language");
        
        script.languageBtn = langBtn;

        // TEST: CJK Character Check (Bottom Left)
        var testCharObj = new GameObject("TestChar_CJK");
        testCharObj.transform.SetParent(root.transform, false);
        var testCharTxt = testCharObj.AddComponent<TextMeshProUGUI>();
        testCharTxt.text = "國"; // Hanja/Kanji for Country
        testCharTxt.fontSize = 60;
        testCharTxt.color = Color.white;
        testCharTxt.alignment = TextAlignmentOptions.Center;

        // Add LocalizedText for Font Switching (No Key=No Text Change)
        testCharObj.AddComponent<LocalizedText>();
        
        var testCharRect = testCharObj.GetComponent<RectTransform>();
        testCharRect.anchorMin = new Vector2(0, 0);
        testCharRect.anchorMax = new Vector2(0, 0);
        testCharRect.pivot = new Vector2(0, 0);
        testCharRect.anchoredPosition = new Vector2(20, 20); // Bottom Left
        testCharRect.sizeDelta = new Vector2(100, 100);

        // Language Popup (Instance)
        var langPopupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Battle/LanguagePopup.prefab");
        if (langPopupPrefab != null)
        {
            var langPopupObj = (GameObject)PrefabUtility.InstantiatePrefab(langPopupPrefab);
            langPopupObj.transform.SetParent(root.transform, false);
            langPopupObj.SetActive(false); // Hidden by default
            script.languagePopup = langPopupObj.GetComponent<LanguagePopup>();
        }
        else
        {
            Debug.LogError("LanguagePopup prefab not found!");
        }

        SetLayerRecursively(root, LayerMask.NameToLayer("UI"));

        string path = "Assets/Prefabs/Battle/LoginPopup.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        return root;
    }

    // 6. LanguagePopup
    private static GameObject GenerateLanguagePopup()
    {
        GameObject root = new GameObject("LanguagePopup");
        var rt = root.AddComponent<RectTransform>();
        SetFullStretch(rt);
        var script = root.AddComponent<LanguagePopup>();
        
        // Dim BG
        var bg = root.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        
        // Panel
        var panel = new GameObject("Panel");
        panel.transform.SetParent(root.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(400, 300);
        panel.AddComponent<Image>().color = Color.white;
        
        var vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlHeight = false;
        vlg.childControlWidth = false;
        
        // Helper to create buttons (Static Text)
        Button CreateLangBtn(string name, string label)
        {
            var bObj = new GameObject(name);
            bObj.transform.SetParent(panel.transform, false);
            var bRect = bObj.AddComponent<RectTransform>();
            bRect.sizeDelta = new Vector2(200, 50);
            bObj.AddComponent<Image>().color = Color.gray;
            var b = bObj.AddComponent<Button>();
            
            var tObj = new GameObject("Text");
            tObj.transform.SetParent(bObj.transform, false);
            var t = tObj.AddComponent<TextMeshProUGUI>();
            t.text = label;
            t.color = Color.black;
            t.fontSize = 24;
            t.alignment = TextAlignmentOptions.Center;
            SetFullStretch(t.rectTransform);
            
            return b;
        }

        // Helper for Localized Button (Close)
        Button CreateLocBtn(string name, string label, string key)
        {
            var b = CreateLangBtn(name, label);
            // Add LocalizedText to the text object (child 0)
            var tObj = b.transform.GetChild(0).gameObject;
            var loc = tObj.AddComponent<LocalizedText>();
            loc.SetKey(key);
            return b;
        }
        
        script.koreanBtn = CreateLangBtn("KR_Btn", "한국어");
        script.englishBtn = CreateLangBtn("EN_Btn", "English");
        script.japaneseBtn = CreateLangBtn("JP_Btn", "日本語");
        
        // Close Button (Localized)
        var closeBtn = CreateLocBtn("CloseBtn", "Close", "UI_Common_Close");
        var cImg = closeBtn.GetComponent<Image>();
        cImg.color = Color.red;
        script.closeBtn = closeBtn;

        SetLayerRecursively(root, LayerMask.NameToLayer("UI"));

        string path = "Assets/Prefabs/Battle/LanguagePopup.prefab";
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
