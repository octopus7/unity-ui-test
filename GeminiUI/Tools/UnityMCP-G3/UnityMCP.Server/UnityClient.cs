using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UnityMCP.Server
{
    public class UnityClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _unityServerUrl = "http://127.0.0.1:8081"; // Default Unity Editor Server URL

        public UnityClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<object?> SendCommandAsync(string commandType, string targetPath, object args)
        {
            // Serialize args to string because Unity's JsonUtility cannot handle nested objects well
            // and we defined the server counterpart to expect a string "ArgsJson"
            string argsJsonString = JsonSerializer.Serialize(args);

            var command = new
            {
                CommandType = commandType,
                TargetPath = targetPath,
                ArgsJson = argsJsonString
            };

            var json = JsonSerializer.Serialize(command);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(_unityServerUrl, content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                
                // Unity should return a JSON object with 'status' and 'data'
                return JsonSerializer.Deserialize<object>(responseString);
            }
            catch (HttpRequestException ex)
            {
                Console.Error.WriteLine($"[UnityClient] Connection failed: {ex.Message}. Is Unity running?");
                return new { error = "Connection to Unity failed. Please ensure Unity Editor is open and the MCP Plugin is running." };
            }
        }
    }
}
