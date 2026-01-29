using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityMCP.Editor
{
    [Serializable]
    public class UnityCommand
    {
        public string CommandType;
        public string TargetPath;
        public string ArgsJson; // We'll parse this manually based on CommandType
    }

    public static class CommandDispatcher
    {
        public static object Dispatch(string jsonBody)
        {
            var command = JsonUtility.FromJson<UnityCommand>(jsonBody);
            if (command == null) return new { error = "Invalid JSON" };

            // Manually parse ArgsJson because JsonUtility is limited with Dictionary<string, object>
            // For now, we assume the Args are passed as a JSON object inside the "Args" field of the main JSON, 
            // but our UnityClient sends "Args" as an object. 
            // Let's adjust CommandStructure to match UnityClient.
            
            // UnityClient sends: { CommandType="", TargetPath="", Args={} }
            // To parse "Args" as a dynamic dictionary or specific struct, we might need a better JSON parser like Newtonsoft.
            // But to keep it "Pure C#" without external deps if possible, we can define specific argument structs.
            
            switch (command.CommandType)
            {
                case "unity_create_canvas_prefab":
                    return PrefabOps.CreateCanvasPrefab(command.ArgsJson);
                case "unity_add_ui_element":
                    return PrefabOps.AddUiElement(command.ArgsJson);
                case "unity_create_script":
                    return ScriptBuilder.CreateScript(command.ArgsJson);
                case "unity_bind_component":
                    return BinderOps.BindComponent(command.ArgsJson);
                default:
                    return new { status = "error", message = $"Unknown command: {command.CommandType}" };
            }
        }
    }
}
