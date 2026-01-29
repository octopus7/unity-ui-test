# 보스 배틀 UI 검증 가이드

## 설정 (Setup)
1.  Unity 에디터를 엽니다.
2.  컴파일이 완료될 때까지 기다립니다.
3.  `BossBattleUI_Gen` 스크립트가 자동으로 실행되어 `Assets/Prefabs/Battle/` 경로에 다음 프리팹들을 생성했을 것입니다:
    - `BattleListItem`
    - `LoginPopup`
    - `ResultPopup`
    - `DamagePopup`
    - `LobbyView`
4.  `LobbyView` 프리팹을 열거나, 새로운 빈 씬(Scene)을 만들어 `LobbyView`를 드래그하여 배치합니다.
5.  `LocalGameServer.cs`가 활성화되어 있는지 확인합니다. (MonoBehaviour이므로, `Server`라는 이름의 GameObject를 만들고 컴포넌트를 붙여주세요.)
    - **참고**: API 서버는 `localhost:8282`에서 실행됩니다.
    - **조치**: `BattleClient` 컴포넌트도 `Server` 오브젝트나 별도의 `Network` 오브젝트에 추가해주세요. (`BattleClient`는 싱글톤이지만 초기화가 필요할 수 있습니다.)

## 테스트 단계 (Test Steps)
1.  **플레이 모드 시작**: Play 버튼을 누릅니다.
2.  **서버 시작 확인**: 콘솔 로그에 `[LocalGameServer] Server Started at http://localhost:8282/`가 뜨는지 확인합니다.
3.  **로그인**:
    - 파란색 배경의 `LoginPopup`이 보여야 합니다.
    - 유저 ID(예: "Hero1")를 입력하고 "GAME START"를 클릭합니다.
    - 콘솔에 `Logged in as Hero1` 로그가 찍히며, 로그인 팝업이 사라지고 로비가 나옵니다.
4.  **로비 화면**:
    - 골드가 0인지 확인합니다.
    - 목록이 비어있을 수 있습니다.
    - **Refresh (새로고침)** 버튼을 클릭합니다.
    - **로직 확인**: 서버는 활성 전투가 10개 미만일 경우 가짜(Dummy) 전투 10개를 자동 생성합니다.
    - 스크롤 뷰에 10개의 전투 항목이 나타나는지 확인합니다.
5.  **전투 생성 (Create Battle)**:
    - "Create Battle" 버튼을 클릭합니다.
    - 목록 최하단(또는 최상단)에 내 ID로 생성된 항목이 추가됩니다.
    - 해당 항목에는 **[Icon]** (하늘색)이 표시되어 내 전투임을 알려줍니다.
6.  **전투 참여 (Participate)**:
    - 아무 전투나 골라 "Participate" 버튼을 클릭합니다.
    - **지연 시간**: 약 500ms의 의도된 지연 시간을 느낄 수 있습니다.
    - **결과 확인**:
        - **일반**: 빨간색 `DamagePopup` 텍스트가 뜨며 데미지가 표시되고, HP 슬라이더가 줄어듭니다.
        - **참가상**: 만약 5회 공격에 실패(HP가 남음)했다면, "참가상(Participation Prize)" 팝업이 뜨고 1골드를 획득합니다.
        - **승리**: 만약 보스를 처치(HP <= 0)했다면, "승리(Victory)" 팝업이 뜨고 100골드를 획득합니다.
7.  **데이터 유지 (Persistence)**:
    - 플레이를 중지합니다.
    - 다시 플레이를 시작합니다.
    - 동일한 ID("Hero1")로 로그인합니다.
    - 획득했던 골드가 유지되어 있는지 확인합니다.
    - 전투 목록 상태가 이전과 동일한지 확인합니다 (만료되거나 종료된 전투 제외).
