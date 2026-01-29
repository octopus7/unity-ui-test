# Unity UI Prefab Generation Workflow
**Unity UI 프리팹 생성 워크플로우**

This document describes the standard workflow for programmatically creating Unity UI prefabs with AI Agents.
이 문서는 AI Agent와 함께 Unity UI 프리팹을 코드로 생성하는 표준 워크플로우를 설명합니다.

---

## Workflow Overview (Script Injection Method)
## 워크플로우 개요 (Script Injection 방식)

This method avoids serialization limitations of MCP by using a **Generator Script** to create prefabs safely and accurately.
MCP의 직렬화 한계나 복잡한 리플렉션 없이, **Generator Script**를 통해 프리팹을 안전하고 정확하게 생성하는 방식입니다.

### Core Principle (핵심 원리)
1.  **Write Both Scripts**: Create the **Runtime Script** (UI Logic) and **Editor Generator Script** (Creation Logic) simultaneously.
    (실제 UI 로직인 **Runtime Script**와 생성 로직인 **Editor Generator Script**를 동시에 작성합니다.)
2.  **Compilation**: Unity compiles both scripts.
    (Unity 컴파일러가 두 스크립트를 컴파일합니다.)
3.  **Auto-Execution**: The Generator Script with `[InitializeOnLoad]` runs automatically to create the prefab and bind references directly.
    (`[InitializeOnLoad]`가 붙은 Generator Script가 자동으로 실행되어 프리팹을 생성하고 필드를 직접 바인딩합니다.)

---

## How to Instruct AI
## AI에게 지시하는 법

When creating a new UI, instruct the AI as follows:
새로운 UI를 만들 때 AI에게 다음과 같이 지시하세요:

> **"Create a [Feature Name] popup using the Unity UI Prefab Generation Workflow."**
> **"Unity UI 프리팹 생성 워크플로우로 [기능명] 팝업을 만들어줘."**
>
> **Requirements (요구사항):**
> 1. Runtime Script: `MyPopup.cs` (Variables: titleText, closeButton, etc.)
> 2. Generator Script: `MyPopupGen.cs` (Path: Assets/Editor)
> 3. Method: Use `[InitializeOnLoad]` and direct reference assignment.
>    (방식: `[InitializeOnLoad]` 사용, 직접 참조 할당)

---

## Example Code (예제 코드)

### 1. Runtime Script (`Assets/Scripts/MyPopup.cs`)
```csharp
using UnityEngine;
using UnityEngine.UI;

public class MyPopup : MonoBehaviour
{
    public Text titleText;
    public Button closeButton;
}
```

### 2. Generator Script (`Assets/Editor/MyPopupGen.cs`)
```csharp
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[InitializeOnLoad]
public class MyPopupGen
{
    static MyPopupGen()
    {
        EditorApplication.delayCall += Generate;
    }

    private static void Generate()
    {
        // Prevent infinite loop (무한 루프 방지)
        if (SessionState.GetBool("MyPopupGen_Done", false)) return;
        SessionState.SetBool("MyPopupGen_Done", true);

        GameObject root = new GameObject("MyPopup");
        var script = root.AddComponent<MyPopup>();
        
        // UI Creation & Binding (UI 생성 및 바인딩)
        var txtObj = new GameObject("Title");
        var text = txtObj.AddComponent<Text>();
        script.titleText = text; // Direct Assignment (직접 할당)
        
        PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/MyPopup.prefab");
        Object.DestroyImmediate(root);
    }
}
```

---

## Advantages (장점)
- **Perfect Binding (완벽한 바인딩)**: No `null` reference errors as assignments are done via code, not by string search.
  (이름으로 찾지 않고 코드로 직접 할당하므로 `null` 참조 오류가 없습니다.)
- **Automation (자동화)**: Prefabs are created immediately after file creation and compilation.
  (파일 생성 즉시 유니티가 컴파일하고 프리팹을 만들어냅니다.)
- **Easy Maintenance (유지보수 용이)**: The prefab generation logic remains as code, making it easy to modify.
  (프리팹 생성 로직 자체가 코드로 남아 있어 수정이 쉽습니다.)
