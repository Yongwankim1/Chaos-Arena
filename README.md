# Chaos Arena

Unity 기반의 3D 멀티플레이 아레나 액션 게임 프로토타입입니다.
플레이어는 로비에서 방을 만들거나 참가한 뒤 캐릭터를 선택하고, 제한된 아레나 안에서 팀 단위 전투를 진행합니다.

## 프로젝트 개요

- 장르: 3D 멀티플레이 아레나 액션
- 엔진: Unity 6000.3.11f1
- 네트워크: Photon Fusion, Photon Chat
- 렌더 파이프라인: Universal Render Pipeline
- 주요 흐름: 로비 접속 -> 방 생성/참가 -> 캐릭터 선택 -> 라운드 전투 -> 결과 표시

## 주요 기능

### 멀티플레이 로비

- Photon Fusion 기반 세션 생성 및 참가
- 로비/방 채팅 기능
- 방 유저 목록 및 준비 상태 UI
- 닉네임 입력과 플레이어 데이터 관리

### 캐릭터 선택 및 클래스

- 팀 기반 캐릭터 선택
- 클래스별 능력치와 스킬 데이터 관리
- 현재 구현 클래스
  - Assassin
  - Mage
  - Brute

### 전투 시스템

- 기본 공격 콤보
- 클래스별 대시 및 이동 스킬
- Q, E, R 스킬 구조
- 피격 판정, 투사체, 범위 공격 처리
- 히트 스톱, 카메라 쉐이크, 데미지 비네트 등 전투 피드백

### 라운드 진행

- 라운드 상태 관리
- 라운드 HUD 및 결과 UI
- 팀/플레이어 체력 표시
- 경기 종료 후 로비 복귀 흐름

### 몬스터 및 AI

- ScriptableObject 기반 적 데이터
- NavMesh 기반 이동
- Behavior 시스템을 활용한 감지, 추적, 공격 패턴
- 근거리/원거리 공격 타입
- 적 체력 UI 및 사망 처리

### 버프 시스템

- Red, Blue, Defence 버프 타입
- ScriptableObject 기반 버프 데이터
- 캐릭터별 버프 적용 인터페이스
- 네트워크 동기화 기반 버프 효과

### 사운드 및 UI

- 캐릭터별 스킬 사운드 라이브러리
- BGM, UI, 내레이션 사운드 관리
- 인게임 HUD, 스킬 가이드, 설정 UI
- 월드 체력바와 타겟 표시 UI

## 조작 방법

| 동작 | 키 |
| --- | --- |
| 이동 | WASD / 방향키 |
| 점프 | Space |
| 대시 | Left Shift |
| 기본 공격 | 마우스 좌클릭 / F |
| Q 스킬 | Q |
| E 스킬 | E |
| R 스킬 | R |

## 실행 방법

1. Unity Hub에서 프로젝트를 엽니다.
2. Unity 버전은 `6000.3.11f1`을 사용합니다.
3. `Assets/01.Scenes/01.LobbyScene.unity` 씬을 엽니다.
4. Play 버튼을 눌러 로비에서 테스트를 시작합니다.

빌드에 포함된 주요 씬은 다음과 같습니다.

- `Assets/01.Scenes/01.LobbyScene.unity`
- `Assets/01.Scenes/03.Map_0(DungeonArena).unity`
- `Assets/01.Scenes/03.Map_1(CharacterTestArena).unity`
- `Assets/01.Scenes/03.Map_2(MiniArena).unity`

## 폴더 구조

```text
Assets
├── 01.Scenes        # 로비, 게임 맵, 테스트 씬
├── 02.Scripts       # 게임 플레이, 네트워크, UI, 전투 로직
├── 03.Prefabs       # 캐릭터, 적, UI, 이펙트 프리팹
├── 04.Data          # 사운드, 버프, 적 데이터 에셋
├── 05.Behavior      # 몬스터 AI Behavior 에셋
├── 06.Animator      # 캐릭터/몬스터 애니메이터
├── 07.Sounds        # 사운드 리소스
├── 09.Scriptable    # ScriptableObject 에셋
└── 99.UI_Images     # UI 이미지 리소스
```

## 사용 패키지

- Photon Fusion
- Photon Chat
- Unity Input System
- Unity AI Navigation
- Unity Behavior
- Cinemachine
- Addressables
- Universal Render Pipeline
- TextMesh Pro

## 현재 구현 범위

이 저장소는 완성 제품이 아닌 프로토타입 단계입니다. 현재는 멀티플레이 접속, 로비/방 흐름, 캐릭터 선택, 라운드 진행, 클래스별 전투, 몬스터 AI, 버프, UI/사운드의 핵심 기능 검증을 목표로 합니다.

향후 개선 후보는 다음과 같습니다.

- 매치메이킹 및 방 검색 UX 개선
- 캐릭터 밸런싱
- 스킬 이펙트와 사운드 polish
- 네트워크 예외 상황 처리 강화
- 맵/모드 추가
- 튜토리얼 및 플레이 가이드 보강
