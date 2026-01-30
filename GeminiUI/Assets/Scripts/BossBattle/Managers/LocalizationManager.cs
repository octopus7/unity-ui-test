using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public enum GameLanguage
{
    Korean,
    English,
    Japanese
}

public class LocalizationManager : MonoBehaviour
{
    private static LocalizationManager _instance;
    public static LocalizationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("LocalizationManager");
                _instance = go.AddComponent<LocalizationManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private Dictionary<string, Dictionary<GameLanguage, string>> _localizationData;
    public GameLanguage CurrentLanguage { get; private set; }
    
    public event Action OnLanguageChanged;

    private bool _isSystemLoaded = false;
    private bool _isContentLoaded = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSystem(); // Always load system on Awake
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Phase 1: System Strings (Login, Settings)
    public void LoadSystem()
    {
        if (_isSystemLoaded) return;

        _localizationData = new Dictionary<string, Dictionary<GameLanguage, string>>();
        
        // 1. Load Language Setting
        if (PlayerPrefs.HasKey("Language"))
        {
            CurrentLanguage = (GameLanguage)PlayerPrefs.GetInt("Language");
        }
        else
        {
            // Auto-detect
            SystemLanguage osLang = Application.systemLanguage;
            switch (osLang)
            {
                case SystemLanguage.Korean:
                    CurrentLanguage = GameLanguage.Korean;
                    break;
                case SystemLanguage.Japanese:
                    CurrentLanguage = GameLanguage.Japanese;
                    break;
                default:
                    CurrentLanguage = GameLanguage.English;
                    break;
            }
        }

        // 2. Load System CSV
        ParseCSV("system");
        _isSystemLoaded = true;
        Debug.Log($"LocalizationManager: System Data Loaded. Language: {CurrentLanguage}");
    }

    // Phase 2: Content Strings (Items, In-Game) - Explicit Call
    public void LoadContent()
    {
        if (_isContentLoaded) return;
        
        ParseCSV("lang");
        _isContentLoaded = true;
        Debug.Log("LocalizationManager: Content Data Loaded.");
        
        // Trigger update to refresh any UI that might be waiting (though usually loaded before entering game)
        OnLanguageChanged?.Invoke();
    }

    private void ParseCSV(string resourceName)
    {
        TextAsset csvFile = Resources.Load<TextAsset>(resourceName);
        if (csvFile == null)
        {
            Debug.LogError($"LocalizationManager: '{resourceName}.csv' not found in Resources.");
            return;
        }

        StringReader reader = new StringReader(csvFile.text);
        string line = reader.ReadLine(); // Header
        
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = SplitCsvLine(line);
            if (values.Count < 4) continue;

            string key = values[0];
            string ko = values[1];
            string en = values[2];
            string jp = values[3];

            if (!_localizationData.ContainsKey(key))
            {
                _localizationData[key] = new Dictionary<GameLanguage, string>();
            }

            // Overwrite or Add
            _localizationData[key][GameLanguage.Korean] = ko;
            _localizationData[key][GameLanguage.English] = en;
            _localizationData[key][GameLanguage.Japanese] = jp;
        }
    }
    
    // Helper to split CSV line respecting quotes
    private List<string> SplitCsvLine(string line)
    {
        List<string> values = new List<string>();
        bool inQuotes = false;
        string currentValue = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue);
                currentValue = "";
            }
            else
            {
                currentValue += c;
            }
        }
        values.Add(currentValue);
        return values;
    }

    // Font Management
    private Dictionary<GameLanguage, TMP_FontAsset> _fonts;

    public void LoadFonts()
    {
        _fonts = new Dictionary<GameLanguage, TMP_FontAsset>();
        
#if UNITY_EDITOR
        string rootPath = "Assets/Fonts/";
        _fonts[GameLanguage.Korean] = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(rootPath + "NotoSansKR-Regular SDF.asset");
        _fonts[GameLanguage.English] = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(rootPath + "NotoSansKR-Regular SDF.asset"); 
        _fonts[GameLanguage.Japanese] = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(rootPath + "NotoSansJP-Regular SDF.asset");
#else
        // TODO: Implement Bundle Loading for builds
        return;
#endif
        
        Debug.Log("LocalizationManager: Fonts Loaded.");
    }

    public TMP_FontAsset GetFont(GameLanguage lang)
    {
        if (_fonts == null) LoadFonts();
        
        if (_fonts.TryGetValue(lang, out var font) && font != null)
        {
            return font;
        }
        // Fallback to KR if specific font miss
        if (_fonts.TryGetValue(GameLanguage.Korean, out var defFont))
        {
            return defFont;
        }
        return null; // TMP will use default if null
    }

    public string GetString(string key)
    {
        if (!_isSystemLoaded) LoadSystem();
        
        if (_localizationData.TryGetValue(key, out var dict))
        {
            if (dict.TryGetValue(CurrentLanguage, out string value))
            {
                return value;
            }
            // Fallback to English
            if (dict.TryGetValue(GameLanguage.English, out string enValue))
            {
                return enValue;
            }
        }
        
        return key; // Return key if not found
    }

    public void SetLanguage(GameLanguage lang)
    {
        if (!_isSystemLoaded) LoadSystem();
        
        if (CurrentLanguage != lang)
        {
            CurrentLanguage = lang;
            PlayerPrefs.SetInt("Language", (int)lang);
            PlayerPrefs.Save();
            
            OnLanguageChanged?.Invoke();
        }
    }
}
