using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ApplyInventoryStyles : MonoBehaviour
{
    [MenuItem("Tools/Apply Inventory Styles")]
    public static void ApplyStyles()
    {
        string prefabPath = "Assets/Prefabs/Battle/InventoryPopup.prefab";
        GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);

        try
        {
            // Load Sprites
            Sprite invenBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/UI/inven_bg.png");
            Sprite greenGlass = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/UI/green_glass.png");
            Sprite btnRed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/UI/btn_red.png");

            if (invenBg == null || greenGlass == null || btnRed == null)
            {
                Debug.LogError("Failed to load one or more sprites.");
                return;
            }

            // apply Panel
            Transform panel = FindDeepChild(prefabContents.transform, "Panel");
            if (panel != null)
            {
                Image panelImg = panel.GetComponent<Image>();
                if (panelImg != null)
                {
                    panelImg.sprite = invenBg;
                    panelImg.color = Color.white;
                    Debug.Log("Applied inven_bg to Panel");
                }
            }

            // apply DetailPanel
            Transform detailPanel = FindDeepChild(prefabContents.transform, "DetailPanel");
            if (detailPanel != null)
            {
                Image detailPanelImg = detailPanel.GetComponent<Image>();
                if (detailPanelImg != null)
                {
                    // Transparent 00ffffff
                    detailPanelImg.color = new Color(1f, 1f, 1f, 0f);
                    Debug.Log("Made DetailPanel transparent");
                }

                // DetailPanel-Background
                // Assuming this means a child named 'Background' inside DetailPanel
                Transform detailBg = detailPanel.Find("Background"); 
                if (detailBg != null)
                {
                    Image detailBgImg = detailBg.GetComponent<Image>();
                    if (detailBgImg != null)
                    {
                        detailBgImg.sprite = greenGlass;
                        detailBgImg.color = Color.white;
                        Debug.Log("Applied green_glass to DetailPanel/Background");
                    }
                }
            }

            // apply CloseBtn
            Transform closeBtn = FindDeepChild(prefabContents.transform, "CloseBtn");
            if (closeBtn != null)
            {
                Image closeBtnImg = closeBtn.GetComponent<Image>();
                if (closeBtnImg != null)
                {
                    closeBtnImg.sprite = btnRed;
                    closeBtnImg.color = Color.white;
                    Debug.Log("Applied btn_red to CloseBtn");
                }
            }

            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            Debug.Log("InventoryPopup styles applied successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error applying styles: " + e.Message);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }
    }

    private static Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
