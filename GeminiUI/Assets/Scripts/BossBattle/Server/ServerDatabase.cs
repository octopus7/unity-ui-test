using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ServerDatabase
{
    private static ServerDatabase _instance;
    public static ServerDatabase Instance => _instance ??= new ServerDatabase();

    private Dictionary<string, UserData> _users = new Dictionary<string, UserData>();
    private Dictionary<string, BattleData> _battles = new Dictionary<string, BattleData>();

    private string _savePath;

    public ServerDatabase()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "server_db.json");
        Load();
    }

    [Serializable]
    private class DatabaseWrapper
    {
        public List<UserData> Users;
        public List<BattleData> Battles;
    }

    public void Save()
    {
        DatabaseWrapper wrapper = new DatabaseWrapper
        {
            Users = new List<UserData>(_users.Values),
            Battles = new List<BattleData>(_battles.Values)
        };

        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(_savePath, json);
        Debug.Log($"[ServerDatabase] Saved to {_savePath}");
    }

    public void Load()
    {
        if (!File.Exists(_savePath)) return;

        try
        {
            string json = File.ReadAllText(_savePath);
            DatabaseWrapper wrapper = JsonUtility.FromJson<DatabaseWrapper>(json);

            if (wrapper.Users != null)
            {
                foreach (var user in wrapper.Users)
                {
                    if(!_users.ContainsKey(user.UserId))
                        _users.Add(user.UserId, user);
                }
            }

            if (wrapper.Battles != null)
            {
                foreach (var battle in wrapper.Battles)
                {
                   if(!_battles.ContainsKey(battle.BattleId))
                        _battles.Add(battle.BattleId, battle);
                }
            }
             Debug.Log($"[ServerDatabase] Loaded {_users.Count} users and {_battles.Count} battles.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ServerDatabase] Load Failed: {e.Message}");
        }
    }

    // User Methods
    public UserData GetOrCreateUser(string userId)
    {
        if (_users.TryGetValue(userId, out var user))
        {
            return user;
        }

        user = new UserData { UserId = userId, Gold = 0 };
        _users.Add(userId, user);
        Save();
        return user;
    }

    public void UpdateUserGold(string userId, int amount)
    {
        if (_users.TryGetValue(userId, out var user))
        {
            user.Gold += amount;
            Save();
        }
    }

    // Battle Methods
    public BattleData CreateBattle(string hostId)
    {
        string battleId = Guid.NewGuid().ToString().Substring(0, 8);
        BattleData battle = new BattleData
        {
            BattleId = battleId,
            HostUserId = hostId,
            CurrentHP = 1000,
            MaxHP = 1000,
            AttemptsUsed = 0,
            MaxAttempts = 5,
            ExpiryTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds() + 1800 // 30 min expiry
        };

        _battles.Add(battleId, battle);
        Save();
        return battle;
    }

    public BattleData GetBattle(string battleId)
    {
        if (_battles.TryGetValue(battleId, out var battle))
        {
            return battle;
        }
        return null;
    }

    public void RemoveBattle(string battleId)
    {
        if (_battles.Remove(battleId))
        {
            Save();
        }
    }

    public List<BattleData> GetActiveBattles()
    {
        // Lazy Deletion
        long now = DateTimeOffset.Now.ToUnixTimeSeconds();
        List<string> expired = new List<string>();

        foreach (var kvp in _battles)
        {
            if (kvp.Value.ExpiryTimestamp < now)
            {
                expired.Add(kvp.Key);
            }
        }

        foreach (var id in expired)
        {
            _battles.Remove(id);
        }
        
        if (expired.Count > 0) Save();

        return new List<BattleData>(_battles.Values);
    }
}
