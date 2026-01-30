using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SelectionUI : MonoBehaviour
{
    public Button inventoryBtn;
    public Button battleBtn;
    public LobbyUI lobbyUI;

    private UserData _currentUser;

    public void Initialize(UserData user)
    {
        _currentUser = user;
    }

    void Start()
    {
        if (inventoryBtn != null)
            inventoryBtn.onClick.AddListener(OnInventoryClicked);
        
        if (battleBtn != null)
            battleBtn.onClick.AddListener(OnBattleClicked);
    }

    public InventoryPopup inventoryPopup;

    void OnInventoryClicked()
    {
        if (inventoryPopup != null)
        {
            // Ensure LobbyContainer is hidden
            if (lobbyUI != null && lobbyUI.lobbyPanel != null)
            {
                lobbyUI.lobbyPanel.SetActive(false);
            }

            inventoryPopup.gameObject.SetActive(true);
            // Generate random items for testing
            var items = new List<InventoryPopup.InventoryEntry>();
            var allKeys = ItemManager.Instance.GetAllItemKeys();
            
            if (allKeys.Count > 0)
            {
                int count = Random.Range(5, 20); // Random count between 5 and 20
                for (int i = 0; i < count; i++)
                {
                    int randomKey = allKeys[Random.Range(0, allKeys.Count)];
                    int randomQty = Random.Range(1, 21); // Random quantity 1-20
                    
                    items.Add(new InventoryPopup.InventoryEntry { itemId = randomKey, quantity = randomQty });
                }
            }
            
            inventoryPopup.Setup(items);
            
            // Hide Selection UI
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Inventory feature coming soon! (Popup not assigned)");
        }
    }

    void OnBattleClicked()
    {
        if (lobbyUI != null)
        {
            lobbyUI.Initialize(_currentUser);
            // Assuming LobbyUI components are on the same object as this script's parent or main view
            // But here we might just need to ensure the Lobby "View" is visible.
            // In the current setup, LobbyUI is likely the main view. 
            // If SelectionUI is an overlay, we close it.
            gameObject.SetActive(false);
            
            // If LobbyUI needs to be explicitly activated (e.g. it was hidden or under this)
            // The generated hierarchy will likely have SelectionUI covering LobbyUI.
            // Hiding SelectionUI reveals LobbyUI.
        }
    }
}
