using UnityEngine;
using UnityEngine.UI;

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

    void OnInventoryClicked()
    {
        Debug.Log("Inventory feature coming soon!");
        // TODO: Show Inventory UI
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
