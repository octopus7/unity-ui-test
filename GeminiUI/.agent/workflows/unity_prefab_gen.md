---
description: Unity UI Prefab Generation via Script Injection
---

This workflow defines the standard process for creating Unity UI prefabs programmatically by injecting a generator script. This avoids MCP serialization issues and ensures type-safe bindings.

## Process Overview

1.  **Create Runtime Script**: The C# class for the UI logic (e.g., `MyPopup.cs`).
2.  **Create Generator Script**: A standard, non-interactive C# Editor script (e.g., `MyPopupGenerator.cs`) that constructs the prefab.
3.  **Compilation**: Unity compiles both. The Generator script can reference the Runtime script class.
4.  **Execution**: Run the Generator script to produce the `.prefab` file.

## Detailed Instructions for AI Assistant

When asked to "Generate Unity Prefab" or similar, follow these steps:

### Step 1: Generate Runtime Code
Create the runtime script in `Assets/Scripts/Generated/` (or user-specified path).
*   Inherit from `MonoBehaviour`.
*   Define public fields for bindings (e.g., `public Text titleText;`).

### Step 2: Generate Editor Code
Create the generator script in `Assets/Editor/Generated/`.
*   **Essential**: Use `[MenuItem("Tools/Generate [Name]")]` for manual trigger OR `[InitializeOnLoad]` with a static constructor/delayCall for auto-trigger (ask user preference if unsure, default to MenuItem for safety).
*   **Logic**:
    1.  Create GameObjects and hierarchy (Canvas, Panels, etc.).
    2.  `AddComponent<T>()` for Unity UI elements and the Runtime Script.
    3.  **Binding**: Directly assign references.
        ```csharp
        var script = root.AddComponent<MyPopup>();
        script.titleText = textComponent; // Direct C# binding
        ```
    4.  **Save**: Use `PrefabUtility.SaveAsPrefabAsset(root, path)`.
    5.  **Cleanup**: `DestroyImmediate(root)` after saving.

### Step 3: Trigger
*   Wait for Unity compilation.
*   If auto-trigger was used, the prefab appears automatically.
*   If MenuItem was used, instruct the user to click the menu or run `EditorApplication.ExecuteMenuItem(...)` via MCP if available.

## Example Generator Script Structure

```csharp
using UnityEngine;
using UnityEditor;

public class MyPopupGenerator
{
    [MenuItem("Tools/Generate MyPopup")]
    public static void Generate()
    {
        GameObject root = new GameObject("MyPopup");
        // ... build hierarchy ...
        
        // Add Logic
        var logic = root.AddComponent<MyPopup>();
        
        // Bind
        logic.someField = someComponent;
        
        // Save
        PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/MyPopup.prefab");
        Object.DestroyImmediate(root);
    }
}
```
