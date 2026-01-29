using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BossBattleSceneSetup
{
    [MenuItem("GeminiUI/Setup BossBattle Scene")]
    public static void SetupScene()
    {
        // 1. Setup Canvas & EventSystem
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("LobbyCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create LobbyCanvas");
        }

        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(esObj, "Create EventSystem");
        }

        // 2. Create Server Object
        string serverObjName = "BossBattleServer";
        GameObject serverObj = GameObject.Find(serverObjName);
        if (serverObj == null)
        {
            serverObj = new GameObject(serverObjName);
            Undo.RegisterCreatedObjectUndo(serverObj, "Create Server Object");
        }
        else
        {
             Debug.Log("Server Object already exists.");
        }

        // Add Components if missing
        if (serverObj.GetComponent<LocalGameServer>() == null)
        {
            Undo.AddComponent<LocalGameServer>(serverObj);
        }
        
        if (serverObj.GetComponent<BattleClient>() == null)
        {
            Undo.AddComponent<BattleClient>(serverObj);
        }

        // 3. Instantiate UI Prefab
        string prefabPath = "Assets/Prefabs/Battle/LobbyView.prefab";
        GameObject lobbyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (lobbyPrefab != null)
        {
            // Check if already in scene
            LobbyUI existingUI = Object.FindAnyObjectByType<LobbyUI>();
            if (existingUI == null)
            {
                GameObject uiInstance = (GameObject)PrefabUtility.InstantiatePrefab(lobbyPrefab);
                Undo.RegisterCreatedObjectUndo(uiInstance, "Instantiate LobbyView");
                
                // CRITICAL: Parent to Canvas
                uiInstance.transform.SetParent(canvas.transform, false);
                
                Selection.activeGameObject = uiInstance;
            }
            else
            {
                Debug.Log("LobbyUI already exists in the scene.");
                // Ensure it is under the canvas
                if (existingUI.transform.parent != canvas.transform)
                {
                    existingUI.transform.SetParent(canvas.transform, false);
                }
                Selection.activeGameObject = existingUI.gameObject;
            }
        }
        else
        {
            Debug.LogError($"Could not find prefab at {prefabPath}. Please generate prefabs first.");
        }

        Debug.Log("BossBattle Scene Setup Complete!");
    }
}
