using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI goldText;
    public Button refreshButton;
    public Button createBattleButton;
    public Button backButton; // Back Button

    public RectTransform listContent;
    public BattleListItem listItemPrefab;

    public SelectionUI selectionUI; // Reference to go back
    
    public GameObject lobbyPanel; // The visual content of Lobby

    private UserData _currentUser;

    public void Initialize(UserData user)
    {
        _currentUser = user;
        if(lobbyPanel) lobbyPanel.SetActive(true); // Ensure visible
        UpdateGoldUI();
        OnRefreshClicked();
    }
    
    // ...

    private void OnBackClicked()
    {
        // Hide Lobby Panel, Show Selection
        if(lobbyPanel) lobbyPanel.SetActive(false);
        
        if (selectionUI != null)
        {
            selectionUI.gameObject.SetActive(true);
        }
    }

    private void UpdateGoldUI()
    {
        if (_currentUser != null)
        {
            goldText.text = $"Gold: {_currentUser.Gold:N0} G";
        }
    }
    
    // Wrapper for button click
    void OnRefreshClicked()
    {
        RefreshBattleList();
    }

    async void RefreshBattleList()
    {
        if(refreshButton) refreshButton.interactable = false;
        
        // Clear list
        foreach (Transform child in listContent)
        {
            Destroy(child.gameObject);
        }

        try
        {
            // Re-fetch user data to sync gold
            UserData updatedUser = await BattleClient.Instance.Login(_currentUser.UserId);
            _currentUser = updatedUser;
            UpdateGoldUI();

            BattleListResponse response = await BattleClient.Instance.GetBattleList();

            foreach (var battle in response.Battles)
            {
                BattleListItem item = Instantiate(listItemPrefab, listContent);
                item.transform.localScale = Vector3.one; // Ensure scale is correct
                item.transform.localPosition = Vector3.zero; // Reset pos
                item.Setup(battle, _currentUser.UserId, OnParticipateClicked);
            }
            
            // Force Layout Rebuild
            LayoutRebuilder.ForceRebuildLayoutImmediate(listContent);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load list: {e.Message}\n{e.StackTrace}");
        }
        finally
        {
            if(refreshButton) refreshButton.interactable = true;
        }
    }

    void Start()
    {
        if(lobbyPanel == null)
        {
            var container = transform.Find("LobbyContainer");
            if(container != null) lobbyPanel = container.gameObject;
        }

        if(refreshButton) refreshButton.onClick.AddListener(OnRefreshClicked);
        if(createBattleButton) createBattleButton.onClick.AddListener(OnCreateBattleClicked);
        if(backButton) backButton.onClick.AddListener(OnBackClicked);
    }

    async void OnCreateBattleClicked()
    {
        createBattleButton.interactable = false;
        try
        {
            await BattleClient.Instance.CreateBattle(_currentUser.UserId);
            RefreshBattleList(); // Refresh list after creation
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create battle: {e.Message}");
        }
        finally
        {
            createBattleButton.interactable = true;
        }
    }

    // Callback when Participate is clicked on an item
    async void OnParticipateClicked(string battleId)
    {
        // Block input?
        try
        {
            AttackResult result = await BattleClient.Instance.AttackBattle(battleId, _currentUser.UserId);
            
            // Show Result Popup
            BattleResultManager.Instance.ShowResult(result);
            
            // Refresh list to update UI states
            RefreshBattleList();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Attack Failed: {e.Message}");
        }
    }
}
