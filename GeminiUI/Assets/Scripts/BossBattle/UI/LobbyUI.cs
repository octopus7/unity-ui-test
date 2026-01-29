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
    public RectTransform listContent;
    public BattleListItem listItemPrefab;

    private UserData _currentUser;

    public void Initialize(UserData user)
    {
        _currentUser = user;
        UpdateGoldUI();
        RefreshBattleList();
    }

    void Start()
    {
        refreshButton.onClick.AddListener(RefreshBattleList);
        createBattleButton.onClick.AddListener(OnCreateBattleClicked);
    }

    private void UpdateGoldUI()
    {
        if (_currentUser != null)
        {
            goldText.text = $"Gold: {_currentUser.Gold:N0} G";
        }
    }

    async void RefreshBattleList()
    {
        refreshButton.interactable = false;
        
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
                item.Setup(battle, _currentUser.UserId, OnParticipateClicked);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load list: {e.Message}");
        }
        finally
        {
            refreshButton.interactable = true;
        }
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
