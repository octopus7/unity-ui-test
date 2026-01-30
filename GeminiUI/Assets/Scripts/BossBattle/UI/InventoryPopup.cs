using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryPopup : MonoBehaviour
{
    public Button closeButton;
    public RectTransform content;
    public GameObject itemTemplate; // We might use a template or just text
    public TextMeshProUGUI emptyText; // To show "No items"

    public GameObject selectionUIObj; // To go back

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;

    public void Setup(List<InventoryEntry> items)
    {
        // Clear existing
        foreach (Transform child in content)
        {
            if (child.gameObject != itemTemplate) 
                Destroy(child.gameObject);
        }

        // Clear details
        if (nameText != null) nameText.text = "";
        if (descText != null) descText.text = "";

        if (items == null || items.Count == 0)
        {
            if (emptyText != null) emptyText.gameObject.SetActive(true);
            return;
        }

        if (emptyText != null) emptyText.gameObject.SetActive(false);

        // Populate
        foreach (var entry in items)
        {
             GameObject slot = null;
             if (itemTemplate != null)
             {
                 slot = Instantiate(itemTemplate, content);
                 slot.SetActive(true);
             }
             else
             {
                 continue; 
             }
             
             // Get Data
             var itemData = ItemManager.Instance.GetItem(entry.itemId);
             if (itemData == null) continue;

             // 1. Set Icon
             var images = slot.GetComponentsInChildren<Image>();
             foreach(var img in images)
             {
                 if (img.gameObject.name == "Icon")
                 {
                     img.sprite = ItemManager.Instance.GetItemIcon(itemData.Icon);
                     img.enabled = (img.sprite != null);
                     break;
                 }
             }

             // 2. Set Quantity
             var txts = slot.GetComponentsInChildren<TextMeshProUGUI>();
             foreach(var txt in txts)
             {
                 if (txt.gameObject.name == "Quantity")
                 {
                     txt.text = entry.quantity.ToString();
                     break;
                 }
             }

             // 3. Add Button for Selection
             var btn = slot.GetComponent<Button>();
             if (btn == null) btn = slot.AddComponent<Button>();
             
             int currentId = entry.itemId;
             btn.onClick.AddListener(() => OnItemClicked(currentId));
        }

        // Auto-select first item if available
        if (items.Count > 0)
        {
            OnItemClicked(items[0].itemId);
        }
    }

    void OnItemClicked(int itemId)
    {
        var item = ItemManager.Instance.GetItem(itemId);
        if (item != null)
        {
            if (nameText != null) nameText.text = ItemManager.Instance.GetItemName(itemId);
            if (descText != null) descText.text = ItemManager.Instance.GetItemDesc(itemId);
        }
    }

    [System.Serializable]
    public struct InventoryEntry
    {
        public int itemId;
        public int quantity;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        if (selectionUIObj != null)
            selectionUIObj.SetActive(true);
    }
}
