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
        SessionState.SetBool("BossBattleUI_Gen_Done_V13", false);
        GenerateAll();
    }

    private static void GenerateAll()
    {
        if (SessionState.GetBool("BossBattleUI_Gen_Done_V13", false)) return;
        SessionState.SetBool("BossBattleUI_Gen_Done_V13", true);

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
            var langPopupObj = GenerateLanguagePopup();
            var selectionUIObj = GenerateSelectionUI(); // Generate SelectionUI
            // var inventoryPopupObj = GenerateInventoryPopup(); // Generate InventoryPopup (Moved inside SelectionUI)
            loginPopupObj = GenerateLoginPopup();
            
            if (langPopupObj) Object.DestroyImmediate(langPopupObj);
            if (selectionUIObj) Object.DestroyImmediate(selectionUIObj);
            // if (inventoryPopupObj) Object.DestroyImmediate(inventoryPopupObj); // Cleanup temp

            // Refresh to ensure LoadAssetAtPath finds them
            AssetDatabase.Refresh();

            // 2. Create Lobby View (Wires everything)
            GenerateLobbyView(itemPrefabObj, resultPopupObj, damagePopupObj, loginPopupObj, null);

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

        var btnTxtObj = new GameObject("Text");
        btnTxtObj.transform.SetParent(btnObj.transform, false);
        var btnTxt = btnTxtObj.AddComponent<TextMeshProUGUI>();
        btnTxt.text = "Participate";
        btnTxt.color = Color.white;
        btnTxt.fontSize = 18;
        btnTxt.alignment = TextAlignmentOptions.Center;
        SetFullStretch(btnTxt.rectTransform);
        
        // Localization
        var loc = btnTxtObj.AddComponent<LocalizedText>();
        loc.SetKey("UI_Battle_Participate");

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

    // 7. SelectionUI
    private static GameObject GenerateSelectionUI()
    {
        GameObject root = new GameObject("SelectionUI");
        var rt = root.AddComponent<RectTransform>();
        SetFullStretch(rt);
        var script = root.AddComponent<SelectionUI>();

        // BG - Semi transparent or solid
        root.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.2f);

        // Panel for Buttons
        var panel = new GameObject("Panel");
        panel.transform.SetParent(root.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(500, 300);
        
        var hlg = panel.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 50;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlHeight = false;
        hlg.childControlWidth = false;

        // Button Helper
        Button CreateBtn(string name, string key)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(panel.transform, false);
            var btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(200, 100);
            
            var img = btnObj.AddComponent<Image>();
            img.color = Color.white;
            var btn = btnObj.AddComponent<Button>();
            
            var txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            var txt = txtObj.AddComponent<TextMeshProUGUI>();
            txt.color = Color.black;
            txt.fontSize = 24;
            txt.alignment = TextAlignmentOptions.Center;
            SetFullStretch(txt.rectTransform);
            
            // Localization
            var loc = txtObj.AddComponent<LocalizedText>();
            loc.SetKey(key);
            
            return btn;
        }

        // Inventory Button
        script.inventoryBtn = CreateBtn("InventoryBtn", "UI_Select_Inventory");
        script.inventoryBtn.image.color = new Color(0.5f, 0.5f, 1f); // Blue-ish

        // Battle Button
        script.battleBtn = CreateBtn("BattleBtn", "UI_Select_Battle");
        script.battleBtn.image.color = new Color(1f, 0.5f, 0.5f); // Red-ish

        // Inventory Popup (Generated and Nested)
        var invPopupObj = GenerateInventoryPopup();
        invPopupObj.transform.SetParent(root.transform, false);
        invPopupObj.SetActive(false); // Hidden by default
        script.inventoryPopup = invPopupObj.GetComponent<InventoryPopup>();

        SetLayerRecursively(root, LayerMask.NameToLayer("UI"));

        string path = "Assets/Prefabs/Battle/SelectionUI.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        return root;
    }

    // 8. InventoryPopup
    private static GameObject GenerateInventoryPopup()
    {
        GameObject root = new GameObject("InventoryPopup");
        var rt = root.AddComponent<RectTransform>();
        SetFullStretch(rt);
        var script = root.AddComponent<InventoryPopup>();
        
        // Dim BG
        root.AddComponent<Image>().color = new Color(0, 0, 0, 0.9f);
        
        // Panel (Resized for Details)
        var panel = new GameObject("Panel");
        panel.transform.SetParent(root.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(600, 750); // Increased Height
        panel.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.3f);
        
        // Title
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        var titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
        titleTxt.text = "My Inventory";
        titleTxt.fontSize = 32;
        titleTxt.color = Color.white;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.rectTransform.anchoredPosition = new Vector2(0, 340); // Moved Up
        titleTxt.rectTransform.sizeDelta = new Vector2(300, 50);
        
        var locTitle = titleObj.AddComponent<LocalizedText>();
        locTitle.SetKey("UI_Inventory_Title");

        // Scroll View (Moved Up)
        var svObj = new GameObject("ScrollView");
        svObj.transform.SetParent(panel.transform, false);
        var svRect = svObj.AddComponent<RectTransform>();
        svRect.anchoredPosition = new Vector2(0, 80); // Center-ish top
        svRect.sizeDelta = new Vector2(560, 420); // Tighter fit
        
        var scrollRect = svObj.AddComponent<ScrollRect>();
        svObj.AddComponent<Image>().color = new Color(0,0,0,0.3f);
        
        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(svObj.transform, false);
        var vpRect = viewport.AddComponent<RectTransform>();
        SetFullStretch(vpRect);
        viewport.AddComponent<RectMask2D>();
        scrollRect.viewport = vpRect;
        
        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0); 
        
        var glg = content.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(100, 100);
        glg.spacing = new Vector2(10, 10);
        glg.padding = new RectOffset(10, 10, 10, 10);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 5; // 5 Items per row
        
        content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        scrollRect.content = contentRect;
        scrollRect.vertical = true;
        scrollRect.horizontal = false;
        script.content = contentRect;
        
        // Load Font
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansKR-Regular SDF.asset");

        // --- Detail Area ---
        var detailPanel = new GameObject("DetailPanel");
        detailPanel.transform.SetParent(panel.transform, false);
        var detailRect = detailPanel.AddComponent<RectTransform>();
        detailRect.anchoredPosition = new Vector2(0, -215); 
        detailRect.sizeDelta = new Vector2(550, 150);
        
        // Border (Bright)
        detailPanel.AddComponent<Image>().color = new Color(0.6f, 1f, 1f, 0.8f); // Cyan-ish bright border

        // Background (Dark)
        var detailBgObj = new GameObject("Background");
        detailBgObj.transform.SetParent(detailPanel.transform, false);
        var detailBgRect = detailBgObj.AddComponent<RectTransform>();
        detailBgRect.anchorMin = Vector2.zero;
        detailBgRect.anchorMax = Vector2.one;
        detailBgRect.offsetMin = new Vector2(2, 2); // 2px Border
        detailBgRect.offsetMax = new Vector2(-2, -2);
        detailBgObj.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f); // Dark background

        // Name Text
        var nameObj = new GameObject("ItemName");
        nameObj.transform.SetParent(detailPanel.transform, false);
        var nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchoredPosition = new Vector2(0, 50); // Moved Up
        nameRect.sizeDelta = new Vector2(500, 40);
        var nameTxt = nameObj.AddComponent<TextMeshProUGUI>();
        nameTxt.text = "Item Name";
        nameTxt.fontSize = 26;
        nameTxt.fontStyle = FontStyles.Bold;
        nameTxt.color = Color.yellow;
        nameTxt.alignment = TextAlignmentOptions.Center;
        if (font != null) nameTxt.font = font;
        script.nameText = nameTxt;

        // Desc Text
        var descObj = new GameObject("ItemDesc");
        descObj.transform.SetParent(detailPanel.transform, false);
        var descRect = descObj.AddComponent<RectTransform>();
        descRect.anchoredPosition = new Vector2(0, -30); // Moved Down
        descRect.sizeDelta = new Vector2(520, 90);
        var descTxt = descObj.AddComponent<TextMeshProUGUI>();
        descTxt.text = "Description goes here...";
        descTxt.fontSize = 20;
        descTxt.color = Color.white;
        descTxt.alignment = TextAlignmentOptions.Top;
        descTxt.enableWordWrapping = true;
        if (font != null) descTxt.font = font;
        script.descText = descTxt;

        // Close Button (Moved Down)
        var btnObj = new GameObject("CloseBtn");
        btnObj.transform.SetParent(panel.transform, false);
        var btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchoredPosition = new Vector2(0, -340);
        btnRect.sizeDelta = new Vector2(150, 40);
        btnObj.AddComponent<Image>().color = Color.red;
        var btn = btnObj.AddComponent<Button>();
        script.closeButton = btn;
        
        var btnTxtObj = new GameObject("Text");
        btnTxtObj.transform.SetParent(btnObj.transform, false);
        var btnTxt = btnTxtObj.AddComponent<TextMeshProUGUI>();
        btnTxt.text = "Close";
        btnTxt.fontSize = 24;
        btnTxt.alignment = TextAlignmentOptions.Center;
        btnTxt.color = Color.white;
        if (font != null) btnTxt.font = font;
        SetFullStretch(btnTxt.rectTransform);
        var locBtn = btnTxtObj.AddComponent<LocalizedText>();
        locBtn.SetKey("UI_Common_Close");
        
        // Item Template (Prefab for slot)
        var itemTemplate = new GameObject("ItemTemplate");
        itemTemplate.transform.SetParent(content.transform, false);
        var itemRect = itemTemplate.AddComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(100, 100);

        // Slot BG
        itemTemplate.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.45f); // Dark Slate
        itemTemplate.AddComponent<Button>();

        // Icon
        var iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(itemTemplate.transform, false);
        var iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(8, 25); // Padding for text below
        iconRect.offsetMax = new Vector2(-8, -8);
        iconObj.AddComponent<Image>();

        // Quantity
        var qtyObj = new GameObject("Quantity");
        qtyObj.transform.SetParent(itemTemplate.transform, false);
        var qtyRect = qtyObj.AddComponent<RectTransform>();
        qtyRect.anchorMin = new Vector2(0, 0); 
        qtyRect.anchorMax = new Vector2(1, 0); // Bottom Stretch
        qtyRect.pivot = new Vector2(0.5f, 0);
        qtyRect.anchoredPosition = new Vector2(0, 2);
        qtyRect.sizeDelta = new Vector2(0, 22);
        var qtyTxt = qtyObj.AddComponent<TextMeshProUGUI>();
        qtyTxt.fontSize = 16;
        qtyTxt.alignment = TextAlignmentOptions.Bottom;
        qtyTxt.color = Color.white;
        if (font != null) qtyTxt.font = font;
        // Wait, where is 'font' defined? It was defined later in my previous view.
        // I need to make sure 'font' is defined before this or used later.
        // In previous view: line 666 'var font = ...' was much earlier? No, wait. 
        // In Step 296, 'var font' is at the END of the function essentially?
        // Ah, 'var font = ...' is at line ~730 in Step 296 (adjusted).
        // Let's assume 'font' is available or I load it again.
        
        // Selected Border (Highlight)
        var selObj = new GameObject("SelectedBorder");
        selObj.transform.SetParent(itemTemplate.transform, false);
        var selRect = selObj.AddComponent<RectTransform>();
        SetFullStretch(selRect);
        // Create Glow Border (Single Image)
        // Check if texture exists, if not generate it
        string glowPath = "Assets/Textures/UI/GlowFrame.png";
        if (!File.Exists(glowPath))
        {
            GenerateGlowTexture(glowPath);
            AssetDatabase.Refresh();
        }
        
        var glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(glowPath);
        
        var selImg = selObj.AddComponent<Image>();
        if (glowSprite != null)
        {
            selImg.sprite = glowSprite;
            selImg.type = Image.Type.Sliced; 
        }
        else
        {
            selImg.color = new Color(1f, 1f, 0f, 0.5f); // Fallback color
        }
        
        // Slightly larger than slot to show "Outer" glow
        selRect.offsetMin = new Vector2(-5, -5);
        selRect.offsetMax = new Vector2(5, 5);

        script.itemTemplate = itemTemplate;
        itemTemplate.SetActive(false);

        
        // Empty Text
        var emptyObj = new GameObject("EmptyText");
        emptyObj.transform.SetParent(panel.transform, false);
        var emptyTxt = emptyObj.AddComponent<TextMeshProUGUI>();
        emptyTxt.text = "No Items";
        emptyTxt.fontSize = 24;
        emptyTxt.alignment = TextAlignmentOptions.Center;
        emptyTxt.color = Color.gray;
        emptyTxt.rectTransform.anchoredPosition = Vector2.zero;
        emptyTxt.rectTransform.sizeDelta = new Vector2(300, 50);
        script.emptyText = emptyTxt;
        
        SetLayerRecursively(root, LayerMask.NameToLayer("UI"));
        
        string path = "Assets/Prefabs/Battle/InventoryPopup.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        return root;
    }

    private static void GenerateGlowTexture(string path)
    {
        int size = 128; // Texture size
        int border = 10; // Glow thickness from edge
        float falloff = 2.5f; // Softness
        
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] colors = new Color[size * size];
        Color glowColor = new Color(1f, 0.9f, 0.2f, 1f); // Gold/Yellow

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Distance to nearest edge
                int distL = x;
                int distR = size - 1 - x;
                int distB = y;
                int distT = size - 1 - y;
                
                int minDist = Mathf.Min(distL, Mathf.Min(distR, Mathf.Min(distB, distT)));
                
                float alpha = 0f;
                
                if (minDist < border)
                {
                   // Normalized distance from 0 (edge) to 1 (inner border start)
                   float t = (float)minDist / border; 
                   // Reverse: 0 -> 1 (strong), 1 -> 0 (weak)
                   // Use pow for falloff
                   alpha = Mathf.Pow(1f - t, falloff);
                }
                
                // Add stronger rim at very edge?
                if (minDist < 2) alpha = Mathf.Max(alpha, 0.8f);

                colors[y * size + x] = new Color(glowColor.r, glowColor.g, glowColor.b, alpha);
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        // Ensure directory exists
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Debug.Log($"[BossBattleUI_Gen] Generated Glow Texture at {path}");
        
        // Import settings to ensure it's a Sprite
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteBorder = new Vector4(border, border, border, border); // 9-slice cap
            importer.SaveAndReimport();
        }
    }

    // 5. LobbyView (Main)
    private static void GenerateLobbyView(GameObject itemPrefab, GameObject resPopup, GameObject dmgPopup, GameObject loginPopup, GameObject selectionPopup)
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

        // Container for Lobby Elements (to toggle visibility without hiding children like SelectionUI)
        var lobbyContainer = new GameObject("LobbyContainer");
        lobbyContainer.transform.SetParent(root.transform, false);
        SetFullStretch(lobbyContainer.AddComponent<RectTransform>());
        script.lobbyPanel = lobbyContainer;

        // Top Panel
        var topPanel = new GameObject("TopPanel");
        topPanel.transform.SetParent(lobbyContainer.transform, false); // Parent to Container
        var topRect = topPanel.AddComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0, 1);
        topRect.anchorMax = new Vector2(1, 1);
        topRect.pivot = new Vector2(0.5f, 1);
        topRect.sizeDelta = new Vector2(0, 60);
        topRect.anchoredPosition = Vector2.zero;
        topPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);

        // Back Button (Top Left)
        var backBtnObj = new GameObject("BackBtn");
        backBtnObj.transform.SetParent(topPanel.transform, false);
        var backRect = backBtnObj.AddComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0, 0.5f);
        backRect.anchorMax = new Vector2(0, 0.5f);
        backRect.pivot = new Vector2(0, 0.5f);
        backRect.anchoredPosition = new Vector2(10, 0); 
        backRect.sizeDelta = new Vector2(80, 40); 
        backBtnObj.AddComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
        var backBtn = backBtnObj.AddComponent<Button>();
        script.backButton = backBtn;
        
        var backTxtObj = new GameObject("Text");
        backTxtObj.transform.SetParent(backBtnObj.transform, false);
        var backTxt = backTxtObj.AddComponent<TextMeshProUGUI>();
        backTxt.text = "Back";
        backTxt.alignment = TextAlignmentOptions.Center;
        backTxt.fontSize = 20;
        SetFullStretch(backTxt.rectTransform);
        var locBack = backTxtObj.AddComponent<LocalizedText>();
        locBack.SetKey("UI_Common_Back");

        // Gold Text (Moved Right)
        var goldObj = new GameObject("GoldText");
        goldObj.transform.SetParent(topPanel.transform, false);
        var goldTxt = goldObj.AddComponent<TextMeshProUGUI>();
        goldTxt.text = "Gold: 0";
        goldTxt.color = Color.yellow;
        goldTxt.fontSize = 24;
        goldTxt.rectTransform.anchorMin = new Vector2(0, 0.5f);
        goldTxt.rectTransform.anchorMax = new Vector2(0, 0.5f);
        goldTxt.rectTransform.pivot = new Vector2(0, 0.5f);
        goldTxt.rectTransform.anchoredPosition = new Vector2(110, 0); // Moved after Back button
        goldTxt.rectTransform.sizeDelta = new Vector2(200, 40); 
        
        var locGold = goldObj.AddComponent<LocalizedText>();
        locGold.SetKey("UI_Lobby_Gold");
        script.goldText = goldTxt;

        // Refresh Button
        var refBtnObj = new GameObject("RefreshBtn");
        refBtnObj.transform.SetParent(topPanel.transform, false);
        var refRect = refBtnObj.AddComponent<RectTransform>();
        refRect.anchorMin = new Vector2(1, 0.5f);
        refRect.anchorMax = new Vector2(1, 0.5f);
        refRect.pivot = new Vector2(1, 0.5f);
        refRect.anchoredPosition = new Vector2(-250, 0); 
        refRect.sizeDelta = new Vector2(160, 50); 
        refBtnObj.AddComponent<Image>().color = Color.blue;
        var refBtn = refBtnObj.AddComponent<Button>();
        script.refreshButton = refBtn;
        
        var refTxtObj = new GameObject("Text");
        refTxtObj.transform.SetParent(refBtnObj.transform, false);
        var refTxt = refTxtObj.AddComponent<TextMeshProUGUI>();
        refTxt.text = "Refresh";
        refTxt.alignment = TextAlignmentOptions.Center;
        SetFullStretch(refTxt.rectTransform);
        var locRef = refTxtObj.AddComponent<LocalizedText>();
        locRef.SetKey("UI_Lobby_Refresh");

         // Create Button
        var crtBtnObj = new GameObject("CreateBtn");
        crtBtnObj.transform.SetParent(topPanel.transform, false);
        var crtRect = crtBtnObj.AddComponent<RectTransform>();
        crtRect.anchorMin = new Vector2(1, 0.5f);
        crtRect.anchorMax = new Vector2(1, 0.5f);
        crtRect.pivot = new Vector2(1, 0.5f);
        crtRect.anchoredPosition = new Vector2(-20, 0);
        crtRect.sizeDelta = new Vector2(220, 50); 
        crtBtnObj.AddComponent<Image>().color = new Color(0.8f, 0.4f, 0);
        var crtBtn = crtBtnObj.AddComponent<Button>();
        script.createBattleButton = crtBtn;
        
        var crtTxtObj = new GameObject("Text");
        crtTxtObj.transform.SetParent(crtBtnObj.transform, false);
        var crtTxt = crtTxtObj.AddComponent<TextMeshProUGUI>();
        crtTxt.text = "Create Battle";
        crtTxt.alignment = TextAlignmentOptions.Center;
        SetFullStretch(crtTxt.rectTransform);
        var locCrt = crtTxtObj.AddComponent<LocalizedText>();
        locCrt.SetKey("UI_Lobby_CreateBattle");

        // Scroll View
        var svObj = new GameObject("ScrollView");
        svObj.transform.SetParent(lobbyContainer.transform, false); // Parent to Container
        var svRect = svObj.AddComponent<RectTransform>();
        SetFullStretch(svRect);
        svRect.offsetMin = new Vector2(20, 20);
        svRect.offsetMax = new Vector2(-20, -80); 
        
        var scrollRect = svObj.AddComponent<ScrollRect>();
        svObj.AddComponent<Image>().color = new Color(0,0,0,0.3f);
        
        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(svObj.transform, false);
        var vpRect = viewport.AddComponent<RectTransform>();
        SetFullStretch(vpRect);
        
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
        vlg.childControlHeight = false; 
        vlg.childForceExpandHeight = false; 
        vlg.childControlWidth = true;
        vlg.spacing = 5;
        vlg.padding = new RectOffset(5, 5, 5, 5);
        content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRect;
        scrollRect.vertical = true;
        scrollRect.horizontal = false;
        script.listContent = contentRect;

        // --- WIRING ---
        
        // 1. Selection UI (Intermediate)
        var selectInstance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Battle/SelectionUI.prefab"));
        selectInstance.transform.SetParent(root.transform, false);
        selectInstance.SetActive(false); // Hidden initially
        
        var selectScript = selectInstance.GetComponent<SelectionUI>();
        selectScript.lobbyUI = script; // Bind Selection -> Lobby

        script.selectionUI = selectScript; // Bind Lobby -> Selection (Back)

        // 2. Login Popup (Entry)
        var loginInstance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Battle/LoginPopup.prefab"));
        loginInstance.transform.SetParent(root.transform, false);
        
        var loginScript = loginInstance.GetComponent<LoginPopup>();
        loginScript.selectionUI = selectScript; // Bind Login -> Selection

        // 3. Inventory Popup (Instance)
        var invInstance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Battle/InventoryPopup.prefab"));
        invInstance.transform.SetParent(root.transform, false);
        invInstance.SetActive(false);
        
        var invScript = invInstance.GetComponent<InventoryPopup>();
        invScript.selectionUIObj = selectInstance; // Back to Selection
        
        selectScript.inventoryPopup = invScript; // Bind Selection -> Inventory
        
        SetLayerRecursively(root, LayerMask.NameToLayer("UI"));

        PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Battle/LobbyView.prefab");
        Object.DestroyImmediate(root);
        Object.DestroyImmediate(loginInstance);
        Object.DestroyImmediate(selectInstance);
        Object.DestroyImmediate(invInstance);
    }
}
