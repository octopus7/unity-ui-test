using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class LocalGameServer : MonoBehaviour
{
    private HttpListener _listener;
    private Thread _serverThread;
    private bool _isRunning = false;
    private const string URL = "http://localhost:8282/";

    void Start()
    {
        StartServer();
    }

    void OnDestroy()
    {
        StopServer();
    }

    private void StartServer()
    {
        // Force initialization of ServerDatabase on Main Thread to capture persistentDataPath
        var db = ServerDatabase.Instance; 
        
        _listener = new HttpListener();
        _listener.Prefixes.Add(URL);
        _listener.Start();
        _isRunning = true;

        _serverThread = new Thread(HandleIncomingConnections);
        _serverThread.Start();
        Debug.Log($"[LocalGameServer] Server Started at {URL}");
    }

    private void StopServer()
    {
        _isRunning = false;
        if (_listener != null)
        {
            try { _listener.Stop(); _listener.Close(); } catch { }
        }
        if (_serverThread != null)
        {
            _serverThread.Abort();
        }
    }

    private void HandleIncomingConnections()
    {
        while (_isRunning)
        {
            try
            {
                HttpListenerContext ctx = _listener.GetContext();
                Task.Run(() => ProcessRequest(ctx));
            }
            catch (HttpListenerException)
            {
                // Ignored upon stop
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalGameServer] Listener Error: {e.Message}");
            }
        }
    }

    private async Task ProcessRequest(HttpListenerContext ctx)
    {
        // Artificial Latency
        await Task.Delay(500);

        HttpListenerRequest req = ctx.Request;
        HttpListenerResponse resp = ctx.Response;

        string result = "";
        int statusCode = 200;

        try
        {
            // Note: Since we are in a background thread, we must be careful with Unity APIs.
            // ServerDatabase operations are generally thread-safe (Dictionary/IO), 
            // but we replaced UnityEngine.Random with System.Random where needed.

            if (req.HttpMethod == "POST" && req.Url.AbsolutePath == "/user/login")
            {
                result = HandleLogin(GetBody(req));
            }
            else if (req.HttpMethod == "POST" && req.Url.AbsolutePath == "/battle/create")
            {
                result = HandleCreateBattle(GetBody(req));
            }
            else if (req.HttpMethod == "GET" && req.Url.AbsolutePath == "/battle/list")
            {
                result = HandleBattleList();
            }
            else if (req.HttpMethod == "POST" && req.Url.AbsolutePath == "/battle/attack")
            {
                result = HandleAttack(GetBody(req));
            }
            else
            {
                statusCode = 404;
                result = "Not Found";
            }
        }
        catch (Exception e)
        {
            statusCode = 500;
            result = $"Internal Error: {e.Message}";
            Debug.LogError(result);
        }

        byte[] data = Encoding.UTF8.GetBytes(result);
        resp.StatusCode = statusCode;
        resp.ContentEncoding = Encoding.UTF8;
        resp.ContentLength64 = data.LongLength;

        await resp.OutputStream.WriteAsync(data, 0, data.Length);
        resp.Close();
    }

    private string GetBody(HttpListenerRequest req)
    {
        using (StreamReader reader = new StreamReader(req.InputStream, req.ContentEncoding))
        {
            return reader.ReadToEnd();
        }
    }

    // --- Handlers ---

    private string HandleLogin(string json)
    {
        UserData request = JsonUtility.FromJson<UserData>(json); // Using UserData as request DTO for ID
        UserData user = ServerDatabase.Instance.GetOrCreateUser(request.UserId);
        return JsonUtility.ToJson(user);
    }

    private string HandleCreateBattle(string json)
    {
        CreateBattleRequest req = JsonUtility.FromJson<CreateBattleRequest>(json);
        BattleData battle = ServerDatabase.Instance.CreateBattle(req.HostUserId);
        return JsonUtility.ToJson(battle);
    }

    // Thread-safe Random
    private static System.Random _random = new System.Random(); 

    private string HandleBattleList()
    {
        List<BattleData> battles = ServerDatabase.Instance.GetActiveBattles();
        
        // Dummy Data Generation
        if (battles.Count < 10)
        {
            int needed = 10 - battles.Count;
            for (int i = 0; i < needed; i++)
            {
                string botId = $"Bot_{_random.Next(1000, 9999)}";
                
                // Create dummy battle
                BattleData dummy = ServerDatabase.Instance.CreateBattle(botId);
                
                // Randomize HP
                dummy.CurrentHP = _random.Next(100, 1000);
                dummy.AttemptsUsed = _random.Next(0, 4);
                
                // Save it so it persists like real battles
                ServerDatabase.Instance.Save(); 
            }
            // Refund list after adding dummies
            battles = ServerDatabase.Instance.GetActiveBattles();
        }

        return JsonUtility.ToJson(new BattleListResponse { Battles = battles });
    }

    private string HandleAttack(string json)
    {
        AttackRequest req = JsonUtility.FromJson<AttackRequest>(json);
        BattleData battle = ServerDatabase.Instance.GetBattle(req.BattleId);

        if (battle == null) throw new Exception("Battle Not Found");
        if (battle.CurrentHP <= 0) throw new Exception("Battle already finished");

        int damage = _random.Next(50, 151);
        battle.CurrentHP -= damage;
        battle.AttemptsUsed++;
        
        AttackResult result = new AttackResult();
        result.DamageDealt = damage;
        result.RemainingHP = battle.CurrentHP;
        result.CurrentAttempts = battle.AttemptsUsed;
        result.Success = true;

        // Result Logic
        if (battle.CurrentHP <= 0)
        {
            result.ResultType = "Victory";
            result.RewardGold = 100;
            ServerDatabase.Instance.UpdateUserGold(req.UserId, 100);
            ServerDatabase.Instance.RemoveBattle(battle.BattleId); // Done
        }
        else if (battle.AttemptsUsed >= 5)
        {
             // Fail but reward Logic check
            result.ResultType = "ParticipationReward";
            result.RewardGold = 1;
            ServerDatabase.Instance.UpdateUserGold(req.UserId, 1);
            ServerDatabase.Instance.RemoveBattle(battle.BattleId); // Done
        }
        else
        {
            result.ResultType = "Normal";
            ServerDatabase.Instance.Save();
        }

        return JsonUtility.ToJson(result);
    }
}
