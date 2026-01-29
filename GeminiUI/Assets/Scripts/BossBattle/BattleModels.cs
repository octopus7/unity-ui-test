using System;
using System.Collections.Generic;

[Serializable]
public class BattleData
{
    public string BattleId;
    public string HostUserId;
    public long ExpiryTimestamp; // Unix Timestamp for expiry logic
    public int AttemptsUsed;
    public int MaxAttempts = 5;
    public int CurrentHP;
    public int MaxHP = 1000;
}

[Serializable]
public class UserData
{
    public string UserId;
    public int Gold;
}

[Serializable]
public class CreateBattleRequest
{
    public string HostUserId;
}

[Serializable]
public class AttackRequest
{
    public string BattleId;
    public string UserId;
}

[Serializable]
public class AttackResult
{
    public bool Success;
    public string Message;
    public int DamageDealt;
    public int RemainingHP;
    public int CurrentAttempts;
    public string ResultType; // "Normal", "Victory", "ParticipationReward"
    public int RewardGold;
}

[Serializable]
public class BattleListResponse
{
    public List<BattleData> Battles;
}
