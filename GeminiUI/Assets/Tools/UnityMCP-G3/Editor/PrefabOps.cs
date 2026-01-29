using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UnityMCP.Editor
{
    public static class PrefabOps
    {
        [Serializable]
        public class CreateCanvasArgs
        {
            public string path;
            public string name;
        }

        [Serializable]
        public class AddUiArgs
        {
            public string prefabPath;
            public string parentName; // Path to parent within prefab, e.g. "Background/Panel"
            public string type;
            public string name;
        }

        public static object CreateCanvasPrefab(string argsJson)
        {
            // Note: In a real implementation, we need a better JSON parser than JsonUtility for nested objects passed as string 
            // OR we assume the server sends the args as a sub-string.
            // UnityClient uses JsonSerializer which produces standard JSON.
            // JsonUtility cannot parse "Args": { ... } into a string field directly unescaped.
            // We might need a small wrapper or simple parsing helper.
            // For this iteration, let's assume the hack: The UnityClient should send Args as a stringified JSON if we use JsonUtility for the outer shell.
            // OR, we parse the whole thing as a specific class for each command.
            
            // Refined Approach: JSON parsing in Unity without Newtonsoft is tricky for generic "Args".
            // Let's try to parse the whole body as a specific request type if possible, or use a simple parser.
            // Given the constraints, let's assume specific DTOs for the entire request wrapper? No, CommandDispatcher is generic.
            
            // Let's assume for now that we can handle the parsing. 
            // To unblock: We will implement specific parsing inside Dispatcher later. 
            // For now, let's just write the core logic.
            
            var args = JsonUtility.FromJson<CreateCanvasArgs>(argsJson);
            
            string fullPath = args.path;
            if (!fullPath.EndsWith(".prefab")) fullPath += ".prefab";
            
            GameObject root = new GameObject(args.name);
            root.AddComponent<Canvas>();
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();
            
            PrefabUtility.SaveAsPrefabAsset(root, fullPath);
            GameObject.DestroyImmediate(root);
            
            return new { status = "success", path = fullPath };
        }

        public static object AddUiElement(string argsJson)
        {
            var args = JsonUtility.FromJson<AddUiArgs>(argsJson);
            
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(args.prefabPath);
            Transform parent = prefabContents.transform;
            
            if (!string.IsNullOrEmpty(args.parentName))
            {
                var found = FindDeep(parent, args.parentName);
                if (found != null) parent = found;
                else Debug.LogWarning($"[UnityMCP] Parent '{args.parentName}' not found in prefab. Defaulting to root.");
            }
            
            GameObject newObj = new GameObject(args.name);
            newObj.transform.SetParent(parent, false);
            
            if (args.type == "Panel")
            {
                var img = newObj.AddComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0.39f); // Default semi-transparent
                var rect = newObj.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            else if (args.type == "Text")
            {
                // Try TextMeshPro if available, else standard Text
                // For simplicity, standard Text for now or check reflection
                var text = newObj.AddComponent<Text>();
                text.text = "New Text";
                text.color = Color.black;
            }
            else if (args.type == "Button")
            {
                newObj.AddComponent<Image>();
                newObj.AddComponent<Button>();
                
                // Add text child
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(newObj.transform, false);
                var t = textObj.AddComponent<Text>();
                t.text = "Button";
                t.alignment = TextAnchor.MiddleCenter;
                t.color = Color.black;
                
                var rect = textObj.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                // Add LayoutElement for Auto Layout
                var le = newObj.AddComponent<LayoutElement>();
                le.minWidth = 100;
                le.minHeight = 50;
                le.preferredHeight = 50;
            }
            else if (args.type == "Image")
            {
                 newObj.AddComponent<Image>();
            }
            else if (args.type == "HorizontalLayout")
            {
                var rect = newObj.AddComponent<RectTransform>();
                
                // If parent has VerticalLayoutGroup, let it control the size.
                // Otherwise fill parent.
                // Simple logic: Always add LayoutElement to be flexible
                var le = newObj.AddComponent<LayoutElement>();
                le.flexibleWidth = 1;
                le.flexibleHeight = 1;

                // Default anchors (will be overridden if parent is layout group)
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                var layout = newObj.AddComponent<HorizontalLayoutGroup>();
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = true;
                layout.spacing = 10;
            }
            else if (args.type == "CenteredWindow")
            {
                var img = newObj.AddComponent<Image>();
                img.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark background

                var rect = newObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(800, 600); // Fixed Size Window

                var layout = newObj.AddComponent<VerticalLayoutGroup>();
                layout.childControlWidth = true; // Use Width of children to fill window
                layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false; // Don't force children to equal height
                layout.spacing = 20;
                layout.padding = new RectOffset(20, 20, 20, 20);
            }

            PrefabUtility.SaveAsPrefabAsset(prefabContents, args.prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
            
            return new { status = "success", created = args.name };
        }

        private static Transform FindDeep(Transform parent, string name)
        {
            var result = parent.Find(name);
            if (result != null)
                return result;

            foreach (Transform child in parent)
            {
                result = FindDeep(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
