# 보스 배틀 UI & Unity 내부 HTTP 서버 구현 계획

## 목표 설명
Unity 내부에서 `HttpListener`를 사용하여 로컬 REST API 서버를 구동합니다. 클라이언트는 `UnityWebRequest`를 통해 자신(localhost)에게 요청을 보내며, 실제 서버 환경과 동일한 비동기 통신 구조를 갖춥니다. 데이터는 로컬 JSON 파일로 관리됩니다.

## 사용자 검토 필요
> [!IMPORTANT]
> **서버 구조 확인**:
> - **Internal Server**: Unity 실행 시 백그라운드 스레드에서 `C# HttpListener`가 8282 포트(변경 가능)로 실행됩니다.
> - **데이터 관리**:
>   - `User`: { 식별자(ID), 골드(Gold) }
>   - `Battle`: { 생성자ID, 남은 시간, 진행 카운트(시도 횟수), 남은 체력 }
> - **레이지 삭제 (Lazy Deletion)**: 전투 조회(`GET`) 요청 시 시간이 만료된 전투는 DB에서 자동 삭제되고 "존재하지 않음" 응답을 보냅니다.
> - **유저 ID**: UI에서 입력받으며 `PlayerPrefs`를 통해 로컬 기기에 저장/불러오기 지원.

## 변경 제안

### Assets/Scripts/BossBattle/Server
#### [NEW] [LocalGameServer.cs](file:///d:/github/unity-ui-test/GeminiUI/Assets/Scripts/BossBattle/Server/LocalGameServer.cs)
- `HttpListener`를 사용하여 HTTP 요청 수신.
- `MonoBehaviour`를 상속받아 Unity 라이프사이클(Start/OnDestroy)과 연동.
- 라우팅 처리:
  - `POST /user/login`: 유저 등록/조회.
  - `POST /battle/create`: 전투 생성.
  - `GET /battle/list`: 현재 활성화된 전투 목록 조회 (`List<BattleInfo>`).
    - **가짜 데이터(Dummy Data)**: 조회 시 활성 전투가 10개 미만일 경우, 부족한 수만큼 임의의 가짜 전투(Host: "Bot_XXX", HP: 랜덤)를 생성하여 목록에 포함. 이 가짜 전투들은 **일반 전투와 동일하게 DB(JSON)에 저장**되어 영속성을 가짐.
  - `POST /battle/attack`: 전투 참여(진행) 요청.
    - **로직**:
        - 데미지 계산: 50 ~ 150 랜덤.
        - 체력 감소 및 카운트 증가.
    - **결과 판정**:
        - **승리**: (HP <= 0), 보상 100골드.
        - **참가상**: (Count >= 5 && HP > 0), 보상 1골드 (실패 처리가 아닌 참가 보상).
        - **일반**: 데미지만 입힘.
- **인공 지연(Artificial Latency)**: 실제 서버 느낌을 내기 위해 모든 요청 처리 시 최소 500ms 대기.

#### [NEW] [ServerDatabase.cs](file:///d:/github/unity-ui-test/GeminiUI/Assets/Scripts/BossBattle/Server/ServerDatabase.cs)
- `Dictionary` 기반 메모리 DB + JSON 파일 영구 저장.
- **영속성 보장**: 전투 생성, 공격 결과 등 상태 변경 시 즉시 JSON 파일로 저장(`Save()`)하여 에디터 종료 후 재실행 시에도 데이터가 유지되도록 구현.

### Assets/Scripts/BossBattle/UI
#### [NEW] [Prefabs List]
다음 5종의 프리팹을 제작합니다.
1.  **BattleListItem.prefab**: 스크롤뷰 내 각 전투 항목 (호스트명, HP, 참가 버튼, 내 전투 아이콘).
2.  **LoginPopup.prefab**: 아이디 입력 및 로그인 창.
3.  **ResultPopup.prefab**: 승리/참가상 결과 모달 팝업.
4.  **DamagePopup.prefab**: 데미지 수치 노출용 토스트 메시지.
5.  **LobbyView.prefab**: 로비 전체 레이아웃 (새로고침, 생성 버튼, 골드 패널 등).

#### [NEW] [LobbyUI.cs](file:///d:/github/unity-ui-test/GeminiUI/Assets/Scripts/BossBattle/UI/LobbyUI.cs)
- **레이아웃**:
    - **Top Left**: 내 골드 표시.
    - **Top Right**: "새로고침" 버튼, "전투 생성" 버튼.
    - **Main Area**: `Scroll Rect` (Vertical) - 전투 목록 표시.
- **기능**:
    - `Start()` 시 `/battle/list` 요청 후 스크롤뷰 갱신.
    - "전투 생성" -> `/battle/create` -> 목록 갱신.
    - "새로고침" -> `/battle/list` -> 목록 갱신.

#### [NEW] [BattleListItem.cs](file:///d:/github/unity-ui-test/GeminiUI/Assets/Scripts/BossBattle/UI/BattleListItem.cs)
- **표시 정보**:
    - **[내 전투] 태그**: `Image` 컴포넌트로 구현. Host ID가 내 ID와 같을 경우 활성화.
    - 생성자 ID, 남은 시간, 진행 카운트(n/5), 남은 체력.
- **버튼**: "참가" (Participate).
    - 클릭 시 `/battle/attack` 요청.

#### [NEW] [BattleResultManager.cs](file:///d:/github/unity-ui-test/GeminiUI/Assets/Scripts/BossBattle/UI/BattleResultManager.cs)
- 서버의 `/battle/attack` 응답에 따라 적절한 팝업 표시.

#### [NEW] [ResultPopups.cs](file:///d:/github/unity-ui-test/GeminiUI/Assets/Scripts/BossBattle/UI/ResultPopups.cs)
- **DamagePopup**: 입힌 데미지 표시 (일반 진행).
- **VictoryPopup**: "승리! 보상: 100G".
- **ParticipationPopup**: "아쉽네요. 참가상: 1G".

## 시나리오 및 로직 상세
1.  **전투 진행**:
    - 공격 시: `Count++`, `HP -= Random(Min, Max)`.
2.  **보상 판정 (서버)**:
    - **승리**: `HP <= 0` -> 유저 골드 증가, 전투 종료(삭제).
    - **참가상**: `Count >= 5` 이고 `HP > 0` (실패) -> 참가상 지급, 전투 종료(삭제).
    - **진행 중**: 그 외 -> 상태 정보만 반환.

## 검증 계획
1.  **서버 기동 확인**: 게임 시작 시 로그에 "Server Started at localhost:8282" 확인.
2.  **데이터 저장 확인**: 게임 종료 후 재실행 시 골드 정보 유지 확인.
3.  **전투 로직 확인**:
    - 시간 만료 후 조회 시 삭제 여부 확인.
    - 5회 공격 실패 시 참가상 획득 확인.
    - 보스 처치 시 골드 획득 확인.
