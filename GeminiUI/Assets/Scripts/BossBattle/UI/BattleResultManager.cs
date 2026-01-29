using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleResultManager : MonoBehaviour
{
    public static BattleResultManager Instance { get; private set; }

    public ResultPopup resultPopupPrefab;
    public DamagePopup damagePopupPrefab;
    
    // Parent for popups - usually the Canvas or a dedicated panel
    public Transform popupParent;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowResult(AttackResult result)
    {
        if (result.ResultType == "Normal")
        {
            // Show Damage Popup
            if (damagePopupPrefab != null)
            {
                DamagePopup popup = Instantiate(damagePopupPrefab, popupParent);
                // Randomize position slightly if needed
                popup.Setup(result.DamageDealt);
            }
        }
        else if (result.ResultType == "Victory")
        {
            // Show Victory Popup
            ShowResultPopup("VICTORY!", $"You defeated the boss!\nReward: {result.RewardGold} Gold");
        }
        else if (result.ResultType == "ParticipationReward")
        {
            // Show Participation Popup
            ShowResultPopup("FAILED...", $"Attempts exhausted (5/5).\nParticipation Prize: {result.RewardGold} Gold");
        }
    }

    private void ShowResultPopup(string title, string message)
    {
        if (resultPopupPrefab != null)
        {
             // Check if one is already active or instantiate new
             // For simplicity, instantiate new or reuse single instance
             var popup = Instantiate(resultPopupPrefab, popupParent);
             popup.Setup(title, message);
             popup.gameObject.SetActive(true);
        }
    }
}
