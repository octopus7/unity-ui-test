using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class FixPopupAssets
{
    static FixPopupAssets()
    {
        EditorApplication.delayCall += RunFix;
    }

    private static void RunFix()
    {
        // Prevent infinite loop if we rely on this, but here we just want to run once.
        // We can check a session flag.
        if (SessionState.GetBool("FixPopupAssetsDone", false)) return;
        SessionState.SetBool("FixPopupAssetsDone", true);

        Debug.Log("[FixPopupAssets] Starting Asset Fix...");

        // 1. Fix Texture Importers (Force Sprite)
        FixTextureType("Assets/Textures/Duck.png");
        FixTextureType("Assets/Textures/Goose.png");

        // 2. Load Prefab and Rebuild Bindings
        string prefabPath = "Assets/Prefabs/TwinImageTestPopup.prefab";
        GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);

        if (prefab == null)
        {
            Debug.LogError("[FixPopupAssets] Prefab not found!");
            return;
        }

        // 3. Find References
        var duckImage = prefab.transform.Find("Window/ImageRow/DuckImage")?.GetComponent<UnityEngine.UI.Image>();
        var gooseImage = prefab.transform.Find("Window/ImageRow/GooseImage")?.GetComponent<UnityEngine.UI.Image>();
        var closeButton = prefab.transform.Find("Window/CloseButton")?.GetComponent<UnityEngine.UI.Button>(); // Button is on the object itself
        
        // Note: The logic script might be on Window or Window/Logic depending on previous step.
        // Previous PS script tried binding "Window/Logic".
        // Let's find where the component actually is.
        var logicObj = prefab.transform.Find("Window/Logic");
        TwinImagePopup logic = null;
        if (logicObj) logic = logicObj.GetComponent<TwinImagePopup>();
        
        if (logic == null) 
        {
            Debug.LogWarning("[FixPopupAssets] TwinImagePopup component not found on Window/Logic, checking root or Window...");
            // Fallback check
            logic = prefab.GetComponentInChildren<TwinImagePopup>(true);
        }

        // 4. Assign Sprites
        Sprite duckSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/Duck.png");
        Sprite gooseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/Goose.png");

        if (duckImage && duckSprite) 
        {
            duckImage.sprite = duckSprite;
            Debug.Log("[FixPopupAssets] Assigned Duck Sprite.");
        }
        else Debug.LogError($"[FixPopupAssets] Failed to assign Duck: Image={duckImage}, Sprite={duckSprite}");

        if (gooseImage && gooseSprite) 
        {
            gooseImage.sprite = gooseSprite;
            Debug.Log("[FixPopupAssets] Assigned Goose Sprite.");
        }
        else Debug.LogError($"[FixPopupAssets] Failed to assign Goose: Image={gooseImage}, Sprite={gooseSprite}");

        // 5. Link Script
        if (logic)
        {
            if (duckImage) logic.leftImage = duckImage;
            if (gooseImage) logic.rightImage = gooseImage;
            if (closeButton) logic.closeButton = closeButton;
            Debug.Log("[FixPopupAssets] Linked TwinImagePopup references.");
        }
        else
        {
             Debug.LogError("[FixPopupAssets] TwinImagePopup component is missing entirely!");
        }

        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefab);
        
        Debug.Log("[FixPopupAssets] Done.");
        
        // Clean up self (Optional, but good practice to allow deletion)
        // AssetDatabase.DeleteAsset("Assets/Editor/FixPopupAssets.cs"); 
    }

    private static void FixTextureType(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
                Debug.Log($"[FixPopupAssets] Converted {path} to Sprite.");
            }
        }
    }
}
