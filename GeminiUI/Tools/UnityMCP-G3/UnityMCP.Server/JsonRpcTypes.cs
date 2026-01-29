using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UnityMCP.Server
{
    // Basic JSON-RPC message structure
    public class JsonRpcMessage
    {
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";
    }

    public class JsonRpcRequest : JsonRpcMessage
    {
        [JsonPropertyName("id")]
        public object? Id { get; set; }

        [JsonPropertyName("method")]
        public string Method { get; set; } = "";

        [JsonPropertyName("params")]
        public object? Params { get; set; }
    }

    public class JsonRpcResponse : JsonRpcMessage
    {
        [JsonPropertyName("id")]
        public object? Id { get; set; }

        [JsonPropertyName("result")]
        public object? Result { get; set; }

        [JsonPropertyName("error")]
        public JsonRpcError? Error { get; set; }
    }

    public class JsonRpcError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";
    }

    // MCP Specific Parameters
    public class InitializeParams
    {
        [JsonPropertyName("protocolVersion")]
        public string ProtocolVersion { get; set; } = "";
        
        [JsonPropertyName("capabilities")]
        public object? Capabilities { get; set; }

        [JsonPropertyName("clientInfo")]
        public Implementation? ClientInfo { get; set; }
    }

    public class Implementation
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("version")]
        public string Version { get; set; } = "";
    }

    public class CallToolParams
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("arguments")]
        public JsonElement Arguments { get; set; }
    }
}
