using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginPopup : MonoBehaviour
{
    public TMP_InputField userIdInput;
    public Button startButton;

    public LobbyUI lobbyUI; // Reference to Lobby to switch
    
    [Header("Language Settings")]
    public Button languageBtn;
    public LanguagePopup languagePopup;

    void Start()
    {
        // Load saved ID
        string savedId = PlayerPrefs.GetString("UserId", "");
        userIdInput.text = savedId;

        startButton.onClick.AddListener(OnStartClicked);
        
        if (languageBtn != null)
        {
            languageBtn.onClick.AddListener(OnLanguageClicked);
        }
        
        // Ensure Managers are initialized (Lazy load checks)
        // LocalizationManager.Instance.Load(); // Optional, usage will trigger it
    }

    void OnLanguageClicked()
    {
        if (languagePopup != null)
        {
            languagePopup.gameObject.SetActive(true);
        }
    }

    async void OnStartClicked()
    {
        string userId = userIdInput.text;
        if (string.IsNullOrEmpty(userId)) return;

        startButton.interactable = false;

        try
        {
            UserData user = await BattleClient.Instance.Login(userId);
            
            // Save ID
            PlayerPrefs.SetString("UserId", user.UserId);
            PlayerPrefs.Save();

            Debug.Log($"Logged in as {user.UserId} with {user.Gold} Gold");

            // Load Content Localization (Phase 2)
            LocalizationManager.Instance.LoadContent();

            // Close Login, Open Lobby
            gameObject.SetActive(false);
            if(lobbyUI != null)
            {
                lobbyUI.Initialize(user);
                lobbyUI.gameObject.SetActive(true);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Login Failed: {e.Message}");
            // Show error toast?
        }
        finally
        {
            startButton.interactable = true;
        }
    }
}
