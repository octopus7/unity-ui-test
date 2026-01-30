using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    public int ItemID;
    public string NameKey;
    public string DescKey;
    public string Icon;
    public int Category; // 1, 2, 3, 4
    public int Price;
}

[System.Serializable]
public class ItemDataListWrapper
{
    public List<ItemData> items; 
    // Unity's JsonUtility requires a wrapper for top-level arrays, 
    // BUT our generated JSON is a top-level array. 
    // We might need a helper or use a library like Newtonsoft if available, 
    // or wrap the JSON content before parsing.
}

public class ItemManager : MonoBehaviour
{
    private static ItemManager _instance;
    public static ItemManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ItemManager");
                _instance = go.AddComponent<ItemManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private Dictionary<int, ItemData> _itemDataMap;
    private bool _isInitialized = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void Load()
    {
        if (_isInitialized) return;

        _itemDataMap = new Dictionary<int, ItemData>();
        
        TextAsset jsonFile = Resources.Load<TextAsset>("ItemData");
        if (jsonFile != null)
        {
            // Unity JsonUtility doesn't support top-level arrays.
            // A simple trick is to wrap it.
            string jsonString = "{ \"items\": " + jsonFile.text + " }";
            ItemDataListWrapper wrapper = JsonUtility.FromJson<ItemDataListWrapper>(jsonString);
            
            if (wrapper != null && wrapper.items != null)
            {
                foreach (var item in wrapper.items)
                {
                    if (!_itemDataMap.ContainsKey(item.ItemID))
                    {
                        _itemDataMap.Add(item.ItemID, item);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("ItemManager: 'ItemData.json' not found in Resources.");
        }

        _isInitialized = true;
        Debug.Log($"ItemManager Initialized. Loaded {_itemDataMap.Count} items.");
    }

    private void EnsureInitialized()
    {
        if (!_isInitialized)
        {
            Load();
        }
    }

    public ItemData GetItem(int id)
    {
        EnsureInitialized();
        _itemDataMap.TryGetValue(id, out ItemData item);
        return item;
    }

    public string GetItemName(int id)
    {
        EnsureInitialized();
        if (_itemDataMap.TryGetValue(id, out ItemData item))
        {
            return LocalizationManager.Instance.GetString(item.NameKey);
        }
        return "Unknown Item";
    }

    public string GetItemDesc(int id)
    {
        EnsureInitialized();
        if (_itemDataMap.TryGetValue(id, out ItemData item))
        {
            return LocalizationManager.Instance.GetString(item.DescKey);
        }
        return "";
    }
    
    public Sprite GetItemIcon(string iconName)
    {
#if UNITY_EDITOR
        // Load from AssetDatabase in Editor
        string path = $"Assets/Textures/Items/{iconName}.png";
        return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
#else
        // TODO: Implement AssetBundle loading for builds
        return null;
#endif
    }
}
