using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace UnityMCP.Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var server = new McpServer();
            await server.RunAsync();
        }
    }

    public class McpServer
    {
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly UnityClient _unityClient;

        public McpServer()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
            _unityClient = new UnityClient();
        }

        public async Task RunAsync()
        {
            // Standard Input Loop
            using var stdin = Console.OpenStandardInput();
            using var reader = new StreamReader(stdin);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    await HandleMessageAsync(line);
                }
                catch (Exception ex)
                {
                    LogError($"Error processing message: {ex.Message}");
                }
            }
        }

        private async Task HandleMessageAsync(string json)
        {
            var request = JsonSerializer.Deserialize<JsonRpcRequest>(json, _jsonOptions);
            if (request == null) return;

            // LogRequest(json); // Debugging

            object? responseResult = null;

            switch (request.Method)
            {
                case "initialize":
                    responseResult = new
                    {
                        protocolVersion = "2024-11-05",
                        capabilities = new
                        {
                            tools = new { listChanged = true },
                            resources = new { listChanged = true }
                        },
                        serverInfo = new
                        {
                            name = "UnityMCP-G3",
                            version = "1.0.0"
                        }
                    };
                    break;
                case "notifications/initialized":
                     // No response needed for notifications
                    return;
                case "tools/list":
                    responseResult = GetToolsList();
                    break;
                case "tools/call":
                    responseResult = await HandleToolCall(request);
                    break;
                default:
                    // Unknown method or ping header
                    break;
            }

            if (request.Id != null)
            {
                var response = new JsonRpcResponse
                {
                    Id = request.Id,
                    Result = responseResult
                };
                SendResponse(response);
            }
        }

        private async Task<object> HandleToolCall(JsonRpcRequest request)
        {
            // request.Params is usually a generic object, we need to re-deserialize or cast to CallToolParams
            // For simplicity, let's assume we can get it from the raw JSON or just parse blindly here if using System.Text.Json element
            // A safer way in this simple implementation is to treat Params as JsonElement
            
            if (request.Params is JsonElement paramsElement)
            {
                if (paramsElement.TryGetProperty("name", out var nameProp) && 
                    paramsElement.TryGetProperty("arguments", out var argsProp))
                {
                    string toolName = nameProp.GetString() ?? "";
                    
                    // Route to Unity Client
                    // Map toolName to UnityCommandType if needed, or send directly
                    return await _unityClient.SendCommandAsync(toolName, "", argsProp);
                }
            }
            
            return new { error = "Invalid tool call parameters" };
        }

        private object GetToolsList()
        {
            return new
            {
                tools = new object[]
                {
                    new
                    {
                        name = "unity_create_canvas_prefab",
                        description = "Creates a new Canvas Prefab",
                        inputSchema = new
                        {
                            type = "object",
                            properties = new
                            {
                                path = new { type = "string", description = "Asset path (e.g. Assets/UI/Popup.prefab)" },
                                name = new { type = "string", description = "Name of the prefab" }
                            },
                            required = new[] { "path", "name" }
                        }
                    },
                    new
                    {
                        name = "unity_create_script",
                        description = "Creates a C# script and queues it for attachment. The queue ensures domain reload is handled.",
                        inputSchema = new
                        {
                            type = "object",
                            properties = new
                            {
                                scriptName = new { type = "string" },
                                content = new { type = "string" },
                                prefabPath = new { type = "string", description = "Prefab to attach to after compilation" }
                            },
                            required = new[] { "scriptName", "content", "prefabPath" }
                        }
                    },
                    new
                    {
                        name = "unity_add_ui_element",
                        description = "Adds a UI element (Panel, Text, Button, etc) to a prefab",
                        inputSchema = new
                        {
                            type = "object",
                            properties = new
                            {
                                prefabPath = new { type = "string" },
                                parentName = new { type = "string", description = "Parent object name (empty for root)" },
                                type = new { type = "string", description = "e.g. Panel, Text, Button, Image" },
                                name = new { type = "string" },
                                properties = new { type = "object", description = "Optional properties" }
                            },
                            required = new[] { "prefabPath", "type", "name" }
                        }
                    },
                     new
                    {
                        name = "unity_bind_component",
                        description = "Binds a UI component to a SerializedField in a script (Queued operation)",
                        inputSchema = new
                        {
                            type = "object",
                            properties = new
                            {
                                prefabPath = new { type = "string" },
                                uiElementName = new { type = "string" },
                                scriptName = new { type = "string" },
                                fieldName = new { type = "string" },
                                targetData = new { type = "object", description = "Data determining what to bind" }
                            },
                            required = new[] { "prefabPath", "scriptName", "fieldName" }
                        }
                    }
                }
            };
        }

        private void SendResponse(JsonRpcResponse response)
        {
            var json = JsonSerializer.Serialize(response, _jsonOptions);
            Console.Out.WriteLine(json);
            Console.Out.Flush();
        }

        private void LogError(string message)
        {
             // MCP uses stderr for logs
             Console.Error.WriteLine($"[Error] {message}");
        }
    }
}
