using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BattleListItem : MonoBehaviour
{
    public TextMeshProUGUI hostNameText;
    public TextMeshProUGUI hpText;
    public Slider hpSlider;
    public TextMeshProUGUI statusText; // "Attempts: 3/5" or "Time: 10m"
    public Image myBattleTag; // Icon for "My Battle"
    public Button participateButton;

    private string _battleId;
    private Action<string> _onParticipate;

    public void Setup(BattleData battle, string myUserId, Action<string> onParticipate)
    {
        _battleId = battle.BattleId;
        _onParticipate = onParticipate;

        hostNameText.text = $"Host: {battle.HostUserId}";
        
        // HP
        hpText.text = $"{battle.CurrentHP} / {battle.MaxHP}";
        hpSlider.maxValue = battle.MaxHP;
        hpSlider.value = battle.CurrentHP;

        // Status
        long now = DateTimeOffset.Now.ToUnixTimeSeconds();
        long remaining = battle.ExpiryTimestamp - now;
        string timeStr = remaining > 0 ? $"{remaining / 60}m Left" : "Expiring...";
        statusText.text = $"Attempts: {battle.AttemptsUsed}/{battle.MaxAttempts} | {timeStr}";

        // My Battle Tag
        if (myBattleTag != null)
        {
            bool isMine = battle.HostUserId == myUserId;
            myBattleTag.gameObject.SetActive(isMine);
        }

        participateButton.onClick.RemoveAllListeners();
        participateButton.onClick.AddListener(() => _onParticipate?.Invoke(_battleId));
    }
}
