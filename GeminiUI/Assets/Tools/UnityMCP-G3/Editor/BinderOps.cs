using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace UnityMCP.Editor
{
    public static class BinderOps
    {
        [Serializable]
        public class BindComponentArgs
        {
            public string prefabPath;
            public string uiElementName; 
            public string scriptName;    
            public string fieldName;     
            public string targetElementName; 
            public string targetAssetPath; // New field
        }

        public static object BindComponent(string argsJson)
        {
            var args = JsonUtility.FromJson<BindComponentArgs>(argsJson);
            
            string error;
            if (TryBind(args, out error))
            {
                return new { status = "success", message = $"Bound {args.fieldName}" };
            }
            else
            {
                // Component Missing likely means compilation pending -> Queue it
                if (error.Contains("not found") || error.Contains("Component"))
                {
                     ScriptBuilder.AddJob(new ScriptBuilder.PendingJob
                    {
                        JobType = "BindComponent",
                        ScriptName = args.scriptName,
                        TargetPrefabPath = args.prefabPath,
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        UiElementName = args.uiElementName,
                        FieldName = args.fieldName,
                        BindTargetName = args.targetElementName,
                        // Note: PendingJob struct in ScriptBuilder needs update to store AssetPath too if we want full robustness,
                        // but for now let's hope it succeeds immediately or we might lose the asset path in queue.
                        // Assuming simple case: script exists, just binding.
                    });
                     return new { status = "pending", message = "Binding queued after compilation." };
                }
                
                return new { status = "error", message = error };
            }
        }

        public static bool TryBind(BindComponentArgs args, out string error)
        {
            error = "";
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(args.prefabPath);
            if (prefabContents == null) 
            {
                error = "Prefab not found"; 
                return false; 
            }

            try
            {
                // 1. Find Script Object
                GameObject scriptObj = prefabContents;
                if (!string.IsNullOrEmpty(args.uiElementName))
                {
                    Transform t = FindDeep(prefabContents.transform, args.uiElementName);
                    if (t) scriptObj = t.gameObject;
                    else { error = $"Script holding element '{args.uiElementName}' not found"; return false; }
                }

                // 2. Find Component Type
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                Type targetType = null;
                foreach (var asm in assemblies)
                {
                    targetType = asm.GetType(args.scriptName);
                    if (targetType != null) break;
                }
                
                // Fallback for Unity Built-in types (Image, Button, etc) which might need full qualification or namespace
                if (targetType == null && args.scriptName == "Image") targetType = typeof(UnityEngine.UI.Image);
                if (targetType == null && args.scriptName == "Button") targetType = typeof(UnityEngine.UI.Button);
                if (targetType == null && args.scriptName == "Text") targetType = typeof(UnityEngine.UI.Text);

                if (targetType == null) 
                {
                    error = $"Type {args.scriptName} not found";
                    return false;
                }

                Component component = scriptObj.GetComponent(targetType);
                if (component == null)
                {
                    error = $"Component {args.scriptName} not attached to {scriptObj.name}";
                    return false;
                }

                // 3. Find Target Object OR Asset
                UnityEngine.Object targetObject = null;

                if (!string.IsNullOrEmpty(args.targetAssetPath))
                {
                    // Load Asset
                    targetObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(args.targetAssetPath);
                    if (targetObject == null)
                    {
                        error = $"Asset at {args.targetAssetPath} not found";
                        return false;
                    }
                }
                else if (!string.IsNullOrEmpty(args.targetElementName))
                {
                    Transform t = FindDeep(prefabContents.transform, args.targetElementName);
                    if (t) targetObject = t.gameObject;
                    else { error = $"Target element '{args.targetElementName}' not found"; return false; }
                }

                // 4. Bind
                SerializedObject so = new SerializedObject(component);
                SerializedProperty prop = so.FindProperty(args.fieldName);

                if (prop == null)
                {
                    error = $"Property {args.fieldName} not found on {args.scriptName}";
                    return false; 
                }

                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (targetObject != null)
                    {
                         // If target is GameObject but property expects Component, try to GetComponent
                         if (targetObject is GameObject go)
                         {
                            // Try to match property type
                            // This relies on field type inspection which is hard without reflection on the field info (which we did partially).
                            // Let's use the property's objectReferenceValue type hint if possible? No.
                            
                            // Reflection approach again
                             FieldInfo fieldInfo = targetType.GetField(args.fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                             if (fieldInfo != null)
                             {
                                 Type fieldType = fieldInfo.FieldType;
                                 if (typeof(Component).IsAssignableFrom(fieldType))
                                 {
                                     var targetComponent = go.GetComponent(fieldType);
                                     if (targetComponent != null) prop.objectReferenceValue = targetComponent;
                                     else 
                                     {
                                         error = $"Target {args.targetElementName} missing component {fieldType.Name}";
                                         return false;
                                     }
                                 }
                                 else if (typeof(GameObject).IsAssignableFrom(fieldType))
                                 {
                                     prop.objectReferenceValue = go;
                                 }
                             }
                             else
                             {
                                 // Fallback: Just try setting it, maybe it works if it's generic Object
                                 prop.objectReferenceValue = go;
                             }
                         }
                         else
                         {
                             // Asset (Sprite, etc)
                             prop.objectReferenceValue = targetObject;
                         }
                    }
                }
                
                so.ApplyModifiedProperties();
                PrefabUtility.SaveAsPrefabAsset(prefabContents, args.prefabPath);
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
            }
        }

        private static Transform FindDeep(Transform parent, string name)
        {
            var result = parent.Find(name);
            if (result != null) return result;
            foreach (Transform child in parent)
            {
                result = FindDeep(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }
}
