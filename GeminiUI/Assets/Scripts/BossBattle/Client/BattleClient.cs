using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class BattleClient : MonoBehaviour
{
    private const string BASE_URL = "http://localhost:8282";

    public static BattleClient Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task<UserData> Login(string userId)
    {
        string json = JsonUtility.ToJson(new UserData { UserId = userId });
        return await PostRequest<UserData>("/user/login", json);
    }

    public async Task<BattleData> CreateBattle(string hostUserId)
    {
        string json = JsonUtility.ToJson(new CreateBattleRequest { HostUserId = hostUserId });
        return await PostRequest<BattleData>("/battle/create", json);
    }

    public async Task<BattleListResponse> GetBattleList()
    {
        return await GetRequest<BattleListResponse>("/battle/list");
    }

    public async Task<AttackResult> AttackBattle(string battleId, string userId)
    {
        string json = JsonUtility.ToJson(new AttackRequest { BattleId = battleId, UserId = userId });
        return await PostRequest<AttackResult>("/battle/attack", json);
    }

    private async Task<T> PostRequest<T>(string endpoint, string jsonBody)
    {
        using (UnityWebRequest req = new UnityWebRequest(BASE_URL + endpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            var operation = req.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[BattleClient] Error: {req.error} : {req.downloadHandler.text}");
                throw new Exception(req.error);
            }

            string responseText = req.downloadHandler.text;
            try 
            {
                return JsonUtility.FromJson<T>(responseText);
            }
            catch(Exception e)
            {
                Debug.LogError($"[BattleClient] JSON Parse Error for {endpoint}: {e.Message} \n Response: {responseText}");
                throw;
            }
        }
    }

    private async Task<T> GetRequest<T>(string endpoint)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(BASE_URL + endpoint))
        {
            var operation = req.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (req.result != UnityWebRequest.Result.Success)
            {
                 Debug.LogError($"[BattleClient] Error: {req.error} : {req.downloadHandler.text}");
                throw new Exception(req.error);
            }

            string responseText = req.downloadHandler.text;
             try 
            {
                return JsonUtility.FromJson<T>(responseText);
            }
            catch(Exception e)
            {
                Debug.LogError($"[BattleClient] JSON Parse Error for {endpoint}: {e.Message} \n Response: {responseText}");
                throw;
            }
        }
    }
}
