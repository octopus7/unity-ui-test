using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace UnityMCP.Editor
{
    [InitializeOnLoad]
    public class MCPServerComponent
    {
        private static HttpListener _listener;
        private static bool _isRunning = false;
        private const string URL = "http://127.0.0.1:8081/";
        private static ConcurrentQueue<Action> _mainThreadQueue = new ConcurrentQueue<Action>();

        static MCPServerComponent()
        {
            EditorApplication.update += OnUpdate;
            StartServer();
        }

        private static void OnUpdate()
        {
            while (_mainThreadQueue.TryDequeue(out var action))
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[UnityMCP] Error executing on main thread: {ex}");
                }
            }
        }

        public static void StartServer()
        {
            if (_isRunning) return;

            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add(URL);
                _listener.Start();
                _isRunning = true;
                
                // Start listening asynchronously
                Task.Run(() => ListenLoop());
                
                Debug.Log($"[UnityMCP] Server started at {URL}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityMCP] Failed to start server: {ex.Message}");
            }
        }

        public static void StopServer()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _listener.Stop();
            _listener.Close();
            Debug.Log("[UnityMCP] Server stopped.");
        }

        private static async void ListenLoop()
        {
            while (_isRunning)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    ProcessRequest(context);
                }
                catch (HttpListenerException)
                {
                    // Listener stopped or closed
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[UnityMCP] Listener error: {ex}");
                }
            }
        }

        private static void ProcessRequest(HttpListenerContext context)
        {
            // Read Request Body
            string body;
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                body = reader.ReadToEnd();
            }

            // Dispatch to Main Thread to interact with Unity API
            _mainThreadQueue.Enqueue(() => 
            {
                try 
                {
                    var responseData = CommandDispatcher.Dispatch(body);
                    SendResponse(context, 200, JsonUtility.ToJson(responseData));
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[UnityMCP] Dispatch Error: {ex}");
                    SendResponse(context, 500, $"{{\"error\": \"{ex.Message}\"}}");
                }
            });
        }

        private static void SendResponse(HttpListenerContext context, int statusCode, string responseString)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                context.Response.StatusCode = statusCode;
                context.Response.ContentLength64 = buffer.Length;
                context.Response.ContentType = "application/json";
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityMCP] Send Response Error: {ex.Message}");
            }
        }
    }

    // Placeholder for Dispatcher

}
