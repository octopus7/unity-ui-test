# UnityMCP-G3 Implementation Plan

## 목표 (Goal)
순수 C#을 사용하여 Unity UGUI 프리팹 생성, 변경, 및 인스펙터 제어(스크립트 연결 등)를 수행하는 MCP 시스템 구축. 특히 스크립트 컴파일 대기 시간 문제를 해결하는 비동기 큐 시스템을 포함함.

## 구조 (Architecture)

시스템은 크게 **MCP Server (External)**와 **Unity Plugin (Internal)** 두 부분으로 나뉩니다.

```mermaid
graph TD
    User[AI / Client] <-->|MCP Protocol (Stdio)| MCPServer[UnityMCP-G3 (C# Console)]
    MCPServer <-->|HTTP REST| UnityPlugin[Unity Editor Plugin]
    
    subgraph Unity Editor
        UnityPlugin -->|Execute| CommandHandler
        CommandHandler -->|Immediate| PrefabOps[Prefab Operations]
        CommandHandler -->|Pending| JobQueue[Compilation Queue]
        JobQueue -->|After Compile| BindingOps[Binder & Linker]
    end
```

### 1. MCP Server (C# Console App)
- **역할**: MCP 프로토콜의 End-point. AI의 요청을 받아 Unity가 이해할 수 있는 HTTP 요청으로 변환합니다.
- **기술 스택**: .NET 8 (or compatible), Standard Input/Output integration.
- **주요 기능**:
    - `CallTool`: 요청된 Tool을 파싱하고 Unity API 서버로 전달.
    - `Resources`: Unity 프로젝트 상태(계층 구조 등)를 읽어 반환.

### 2. Unity Editor Plugin (In-Editor Server)
- **역할**: Unity 에디터 내부에서 실제 Asset 조작 및 로직 실행.
- **기술 스택**: `System.Net.HttpListener` (Editor only).
- **모듈 구성**:

#### A. Communication Layer
- **HttpServerComponent**: 로컬 포트(예: 8080)를 열고 MCP Server의 커맨드를 수신.

#### B. Core Logic Layer
- **PrefabGenerator**: `GameObject`, `RectTransform`, `Image`, `Text` 등 UGUI 요소 생성.
- **ScriptBuilder**: 템플릿 기반 C# 스크립트 파일 생성 (`File.WriteAllText`).
- **InspectorBinder**: `SerializedObject`를 사용하여 public 필드/프로퍼티에 다른 객체나 값을 할당.

#### C. Lifecycle Layer (The Compilation Queue)
- **Problem**: 스크립트 생성 직후에는 `Type`이 없어 `AddComponent` 불가능.
- **Solution (Job Queue)**:
    1. **Phase 1**: 스크립트 파일 생성 + 'PendingJob'을 JSON으로 저장.
    2. `AssetDatabase.Refresh()` 호출.
    3. **Phase 2 (`InitializeOnLoad`)**: 컴파일 완료 후 로드되면 저장된 'PendingJob' 실행.
    4. 실제 `AddComponent` 및 `InspectorBinder` 실행.

## 상세 모듈 계획 (Detailed Modules)

### 1. Packet Structure
Unity와 통신하기 위한 공통 데이터 구조.
```csharp
public class UnityCommand 
{
    public string CommandType; // "CreatePrefab", "AddScript", "BindField"
    public string TargetPath;
    public Dictionary<string, object> Args;
}
```

### 2. Tools Specification
AI에게 제공할 MCP 도구 목록.

- `unity_create_canvas_prefab(path, name)`: 기본 캔버스 프리팹 생성.
- `unity_add_ui_element(prefabPath, parentName, type, properties)`: UI 요소(Panel, Text 등) 추가.
- `unity_create_script(scriptName, content)`: 스크립트 파일 생성 (Queue 등록).
- `unity_bind_component(prefabPath, uiElementName, scriptName, fieldName, targetElementName)`: 스크립트 변수에 UI 객체 연결 (Queue 등록 가능성 있음).

## 검증 계획 (Verification Plan)
1. **서버-에디터 통신**: 간단한 "Ping" 명령으로 통신 확인.
2. **프리팹 생성**: 빈 프리팹 -> 캔버스 -> UI 요소 추가 순으로 생성 확인.
3. **컴파일 사이클**: 스크립트 생성 명령 후, 유니티가 컴파일하고 자동으로 컴포넌트가 붙는지 확인.
