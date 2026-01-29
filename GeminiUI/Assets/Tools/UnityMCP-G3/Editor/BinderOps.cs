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
            public string uiElementName; // Element containing the script (e.g. root or child)
            public string scriptName;    // Component Type Name
            public string fieldName;     // Field to bind
            public string targetElementName; // The UI element to assign to the field
        }

        public static object BindComponent(string argsJson)
        {
            var args = JsonUtility.FromJson<BindComponentArgs>(argsJson);

            GameObject prefabContents = PrefabUtility.LoadPrefabContents(args.prefabPath);
            
            // 1. Find the object holding the script
            GameObject scriptObj = prefabContents; // Default to root
            if (!string.IsNullOrEmpty(args.uiElementName))
            {
                var found = prefabContents.transform.Find(args.uiElementName);
                if (found) scriptObj = found.gameObject;
            }

            // 2. Find the component (Script)
            // Need to find type by name
            Component component = scriptObj.GetComponent(args.scriptName); // GetComponent(string) only works for built-in? No, deprecated.
            // We need Type
             var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type targetType = null;
            foreach (var asm in assemblies)
            {
                targetType = asm.GetType(args.scriptName);
                if (targetType != null) break;
            }
            
            if (targetType == null)
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
                return new { status = "error", message = $"Type {args.scriptName} not found" };
            }

            component = scriptObj.GetComponent(targetType);
            if (component == null)
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
                // Return special status to indicate "Not Found (Maybe Pending)"
                return new { status = "error", code = "ComponentNotFound", message = $"Component {args.scriptName} not found on {scriptObj.name}" };
            }

            // 3. Find the target object to bind
            GameObject targetObj = prefabContents;
            if (!string.IsNullOrEmpty(args.targetElementName))
            {
                var found = prefabContents.transform.Find(args.targetElementName);
                if (found) targetObj = found.gameObject;
            }

            // 4. Bind using SerializedObject
            SerializedObject so = new SerializedObject(component);
            SerializedProperty prop = so.FindProperty(args.fieldName);

            if (prop == null)
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
                return new { status = "error", message = $"Property {args.fieldName} not found on {args.scriptName}" };
            }

            if (prop.propertyType == SerializedPropertyType.ObjectReference)
            {
                // Try to find if the property expects a Component or GameObject
                // Reflection to check type? Or just try assigning GameObject or Component
                // SerializedProperty doesn't easily tell us the exact required Component type (e.g. Text vs Image) easily without reflection on the field.
                
                // Naive approach: Try to assign the GameObject. If field expects Component, Unity SerializedProperty *might* handle it if we assign the component?
                // Actually, if we assign a GameObject to a field expecting `Text`, it might fail or auto-find.
                // Best practice: Check field type.
                
                FieldInfo fieldInfo = targetType.GetField(args.fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fieldInfo != null)
                {
                    Type fieldType = fieldInfo.FieldType;
                    if (typeof(Component).IsAssignableFrom(fieldType))
                    {
                        // Field wants a Component (e.g. Text)
                        var targetComponent = targetObj.GetComponent(fieldType);
                        if (targetComponent != null)
                        {
                            prop.objectReferenceValue = targetComponent;
                        }
                        else
                        {
                            // Try generic Image/Text/Button if exact match not found? 
                            // Or just fail.
                             PrefabUtility.UnloadPrefabContents(prefabContents);
                             return new { status = "error", message = $"Target {args.targetElementName} does not have component {fieldType.Name}" };
                        }
                    }
                    else if (typeof(GameObject).IsAssignableFrom(fieldType))
                    {
                        prop.objectReferenceValue = targetObj;
                    }
                }
            }
            else
            {
                 // Non-object property (int, string, etc) - not supported by this simple binder yet
            }

            so.ApplyModifiedProperties();
            PrefabUtility.SaveAsPrefabAsset(prefabContents, args.prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);

            return new { status = "success", message = $"Bound {args.targetElementName} to {args.fieldName}" };
        }
    }
}
