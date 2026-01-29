# Unity MCP Compilation Workflow: "Compilation Queue" Pattern

MCP 스크립트 생성 시 발생하는 컴파일 대기 문제를 해결하기 위한 아키텍처 제안서입니다.

## 핵심 문제 (The Problem)
Unity는 C# 스크립트 변경 시 **비동기 컴파일(Asynchronous Compilation)**을 수행하며, 이 과정에서 **도메인 리로드(Domain Reload)**가 발생합니다.
- MCP가 스크립트 파일을 생성(`File.WriteAllText`)해도, Unity가 컴파일을 완료하고 도메인을 리로드하기 전까지는 해당 `Type`이 존재하지 않습니다.
- 따라서 즉시 이어지는 `AddComponent` 요청은 실패합니다.

## 해결 솔루션: "Job Queue" 시스템 (The "Job Queue" System)

이 솔루션은 "스크립트 생성 및 컴포넌트 부착" 작업을 **2단계**로 분리하고, 도메인 리로드를 생존하는 **작업 큐(Job Queue)**를 사용합니다.

### 워크플로우 (Workflow)

1.  **MCP 요청 (Request)**: "CreateScriptAndAttach(ScriptName, PrefabPath)"
2.  **1단계 처리 (Pre-Compile)**:
    -   MCP가 `.cs` 파일을 생성합니다.
    -   **작업 예약**: `PendingActions.json` 또는 `EditorPrefs`에 "컴파일 후 `ScriptName`을 `PrefabPath`에 부착하라"는 명령을 저장합니다.
    -   `AssetDatabase.Refresh()`를 호출하여 강제 컴파일을 유발합니다.
    -   (옵션) MCP는 클라이언트에 "Pending" 상태를 반환하거나, 처리가 완료될 때까지 Polling 할 수 있는 ID를 반환합니다.
3.  **Unity 컴파일 & 리로드**: Unity 에디터가 스크립트를 컴파일하고 상태를 초기화(Reload) 합니다.
4.  **2단계 처리 (Post-Compile)**:
    -   `[InitializeOnLoad]` 속성이 있는 정적 생성자가 실행됩니다.
    -   저장된 `PendingActions`를 확인합니다.
    -   이제 컴파일이 완료되었으므로 `Type.GetType("ScriptName")`이 성공합니다.
    -   타겟 프리팹에 컴포넌트를 부착하고 저장합니다.
    -   큐를 비웁니다.

---

### 구현 가이드 (Implementation Guide)

#### 1. Pending Action 데이터 구조
Serializable한 데이터 구조를 만듭니다.

```csharp
[Serializable]
public class PendingAction
{
    public string ActionType; // e.g., "AddComponent"
    public string ScriptName;
    public string TargetPrefabPath;
    // 추가 파라미터...
}
```

#### 2. Queue Manager (MCP 서버 측)
요청을 받아 파일을 쓰고 큐에 작업을 추가하는 로직입니다.

```csharp
public void HandleCreateScriptAndAttach(string scriptName, string code, string prefabPath)
{
    // 1. 스크립트 파일 생성
    string path = $"Assets/Scripts/{scriptName}.cs";
    File.WriteAllText(path, code);

    // 2. 작업 큐에 추가
    var action = new PendingAction 
    { 
        ActionType = "AddComponent", 
        ScriptName = scriptName, 
        TargetPrefabPath = prefabPath 
    };
    PendingActionQueue.Enqueue(action); // JSON 파일로 저장 권장 (EditorPrefs는 용량 제한 있음)

    // 3. 컴파일 유발
    AssetDatabase.ImportAsset(path); 
    AssetDatabase.Refresh();
}
```

#### 3. Post-Compile Processor (InitializeOnLoad)
컴파일 후 실행될 처리기입니다.

```csharp
[InitializeOnLoad]
public class MCPJobProcessor
{
    static MCPJobProcessor()
    {
        // 도메인 리로드 직후 실행됨
        ProcessQueue();
    }

    static void ProcessQueue()
    {
        var actions = PendingActionQueue.Load(); // JSON 로드
        if (actions == null || actions.Count == 0) return;

        foreach (var action in actions)
        {
            if (action.ActionType == "AddComponent")
            {
                // 이제 Type을 찾을 수 있음
                System.Type type = System.Type.GetType(action.ScriptName + ", Assembly-CSharp"); 
                if (type == null) 
                {
                    Debug.LogError($"[MCP] Type {action.ScriptName} not found even after compile.");
                    continue;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(action.TargetPrefabPath);
                // 프리팹 수정 모드 진입 필요 없이 바로 수정 가능한 경우 (Project 뷰)
                // 또는 PrefabUtility.LoadPrefabContents 사용
                
                GameObject instance = PrefabUtility.LoadPrefabContents(action.TargetPrefabPath);
                instance.AddComponent(type);
                PrefabUtility.SaveAsPrefabAsset(instance, action.TargetPrefabPath);
                PrefabUtility.UnloadPrefabContents(instance);
                
                Debug.Log($"[MCP] Successfully attached {action.ScriptName} to {action.TargetPrefabPath}");
            }
        }

        PendingActionQueue.Clear();
    }
}
```

## 에이전트/클라이언트 측 전략 (Client-Side Workflow)

만약 MCP 코드를 수정하기 어렵다면, 클라이언트(에이전트)의 행동 패턴을 변경해야 합니다.

1.  **Batch Generation**: 필요한 모든 스크립트를 먼저 생성합니다.
2.  **Explicit Compile Wait**: 
    -   에이전트가 "컴파일 대기" 도구(존재한다면)를 호출하거나, 사용자에게 "컴파일이 끝났나요?"라고 묻습니다.
    -   또는 단순히 `Thread.Sleep` (비추천) 하거나, `CheckTypeExists` 같은 툴로 반복 확인합니다.
3.  **Assembly**: 컴파일 확인 후 `AddComponent`를 일괄 수행합니다.

**추천**: "Job Queue" 시스템을 MCP에 구현하는 것이 가장 매끄러운 경험을 제공합니다.
