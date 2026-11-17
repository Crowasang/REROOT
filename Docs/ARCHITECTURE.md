# 구조

VContainer로 조립한다. 조립 담당은 두 단계다.

- **`RootLifetimeScope`** (앱 전체에 하나, 씬이 바뀌어도 유지): 재화·강화·기록·저장·씬 전환. 프리팹 `Prefabs/Core/RootLifetimeScope`를 `Settings/VContainerSettings`에 Root로 등록해 두어 게임이 켜질 때 자동으로 만들어진다.
- **씬별 LifetimeScope** (`GameLifetimeScope` 등): 그 씬에서만 사는 것. Root를 자동으로 부모로 삼아 Root에 등록된 것을 그대로 받아 쓴다.

각 클래스는 생성자(또는 `[Inject]`)로 **인터페이스만** 받는다. 싱글톤과 `FindObjectOfType`은 쓰지 않는다.

> `VContainerSettings`의 `DisableScriptModifier`는 켜 둔다. 끄면 VContainer가 새로 만든 `*LifetimeScope.cs`를 빈 템플릿으로 덮어쓴다.

## Root 계층 (씬 너머 유지)

| 클래스 | 인터페이스 | 맡는 일 |
|---|---|---|
| `Wallet` | `IWallet`, `ISaveable` | 나무(재화) 늘리기·쓰기 |
| `PlayerRecords` | `IPlayerRecords`, `ISaveable` | 누적 기록(벤 나무, 세션 수) |
| `UpgradeService` | `IUpgradeService`, `ISaveable` | 강화 레벨, 다음 비용, 구매 |
| `StatService` | `IStatService` | 강화 효과를 모아 최종 수치 계산 |
| `SaveService` | `ISaveService` | 시작할 때 불러오기, 저장, 앱 종료 시 저장 |
| `AutoSaveManager` | — | 강화 구매 직후 저장 |
| `SceneLoader` | `ISceneLoader` | 저장 후 씬 전환 (`SceneId`: Title / Game / Ending) |

## 게임 흐름 (상태 머신)

`GameStateMachine`(`IGameStateMachine`)이 지금 단계 하나만 들고 있고, 단계는 `IGameState`(`Enter` / `Exit` / `Tick`)를 구현한 클래스다.

```
GameInitializer → ForestState(벌목) → ResultState(결과) → ForestState …
```

- `ForestState`: 세션을 시작하고, 세션이 끝나면 `ResultState`로 넘긴다.
- `ResultState`: 결과 창을 띄우고, Continue를 누르면 `ForestState`로 넘긴다. 나갈 때 창을 닫는다.
- 단계에 딸린 화면(결과, 강화 등)은 그 상태가 직접 열고 닫는다. 늘 떠 있는 화면(HUD, 일시정지)은 `UIManager`가 맡는다.

## 강화와 수치

- **데이터:** `UpgradeData`(SO, `Data/Upgrades/`) 하나가 강화 하나. id, 최대 레벨, 비용(`baseCost × growth^level`), 효과 목록(`UpgradeEffect`: 어떤 `StatType`을 Flat/Percent로 레벨당 얼마).
- **목록:** `UpgradeCatalog`(SO)에 모아 `RootLifetimeScope` 프리팹에 연결.
- **계산:** `StatService.Evaluate(stat, baseValue)` = `(기본값 + Flat 합) × (1 + Percent 합)`. 레벨이 바뀌면 다시 모으고 `OnChanged`를 알린다.
- **쓰는 쪽:** 기본값은 각자의 SO(`PlayerStatsData`, `SessionSettings`, `TreeData`)에 두고, 쓸 때 `Evaluate`를 거친다. 예: `PlayerStats`(공격력·공속·범위·이동속도), `SessionManager`(세션 시간), `RewardManager`(나무 획득량, 보너스 시간).

## 세이브

- `persistentDataPath/save.json`, Newtonsoft JSON. 임시 파일에 쓴 뒤 교체해서 쓰다 꺼져도 깨지지 않는다. 읽을 수 없는 파일은 `.broken`으로 복사해 두고 새로 시작한다.
- `SaveData`에 `Version`이 있다. 구조를 바꾸면 `CURRENT_VERSION`을 올리고 `SaveService.Migrate`에 옛 버전 변환을 추가한다.
- 저장 대상은 `ISaveable`(`Load` / `Write`)을 구현하고 Root에 등록만 하면 `SaveService`가 모두 모아 처리한다.
- 저장 시점: 세션 종료(`SessionSaveTrigger`), 강화 구매(`AutoSaveManager`), 씬 전환(`SceneLoader`), 앱 종료(`SaveService`).
- 없어진 강화 id는 무시하고, 최대 레벨을 넘는 값은 잘라낸다.

## 게임 씬 매니저 (역할 하나씩)

| 매니저 | 맡는 일 | 의존 |
|---|---|---|
| `GameInitializer` | 씬 시작 시 한 번: 맵 경계, 카메라 범위, 플레이어 생성, 첫 단계(`ForestState`) 진입 | `IMapBoundary`, `ICameraController`, `IPlayerSpawner`, `IGameStateMachine` |
| `SessionManager` (`ISessionService`) | 세션 시작과 끝, 결과 알림 | `IForest`, `ISessionTimer`, `IWallet`, `IPlayerSpawner`, `ICameraController`, `IStatService` |
| `PauseManager` (`IPauseService`) | 일시정지, ESC, `Time.timeScale` | `ISessionService` |
| `RewardManager` | 나무 보상, 시간 보너스 | `IForest`, `IWallet`, `ISessionTimer`, `ISessionService`, `IStatService` |
| `PlayerControlManager` | 플레이어 조작 켜고 끄기 (플레이어 있음 && 세션 중 && 안 멈춤) | `IPlayerSpawner`, `ISessionService`, `IPauseService` |
| `UIManager` | HUD·일시정지 창과 서비스 연결 (MVP의 프레젠터) | 뷰 인터페이스 + 서비스 |
| `AudioManager` | 게임에서 일어나는 일에 맞춰 어떤 소리를 낼지 결정 | `IAudioService`, `IPlayerSpawner`, `IForest`, `ISessionService`, `IPauseService`, 뷰 인터페이스 |
| `RecordManager` | 누적 기록 갱신 | `ISessionService`, `IForest`, `IPlayerRecords` |
| `SessionSaveTrigger` | 세션이 끝나면 저장 | `ISessionService`, `ISaveService` |

매니저끼리는 서로를 직접 호출하기보다 R3 이벤트(`OnStarted`, `OnEnded`, `IsActive`, `IsPaused`)를 구독해서 반응한다.

## 씬 오브젝트 (MonoBehaviour)

- `TreeSpawner` (`IForest`): 숲 깔기·치우기. 자리 계산은 `ForestLayout`(순수 계산), 재사용은 `TreePool`.
- `Trees` (`IDamageable`): 나무 로직(체력, 판정, 쓰러짐 알림). 겉모습 연출은 `TreeView`.
- `PlayerSpawner` (`IPlayerSpawner`): 컨테이너로 플레이어를 만들어 `PlayerStats`를 주입한다.
- `PlayerController` (`IPlayerControl`): 바깥이 플레이어를 다루는 유일한 창구. `PlayerMovement`, `PlayerAttack`, `PlayerAnimator`는 내부 구현.
- `GameCamera` (`ICameraController`), `MapBoundary` (`IMapBoundary`)
- `AudioPlayer` (`IAudioService`): 배경음 1채널과 효과음 여러 채널. 소리 파일과 볼륨은 `SoundLibrary`.
- 뷰: `SessionHud` (`IHudView`), `PausePanel` (`IPauseView`), `SessionEndPanel` (`ISessionEndView`) — 시스템을 모르고 표시와 클릭 알림만 한다.

## 새 기능을 붙이는 법

- **새 강화:** `Data/Upgrades/`에 `UpgradeData`를 만들고 `UpgradeCatalog_01_Default`에 추가한다. 코드는 안 바꾼다.
- **강화가 바꿀 새 수치:** `StatType`에 값을 추가하고, 그 수치를 쓰는 곳에서 `IStatService.Evaluate`를 거치게 한다.
- **새 게임 단계(강화 화면, 로딩 등):** `IGameState`를 구현한 클래스를 만들고 `GameLifetimeScope`에 `Register<T>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf()` 한 줄 추가한다. 넘어가는 쪽에서 `owner.ChangeState<T>()`를 부른다. 예정: `ResultState → UpgradeState → ForestState`.
- **저장할 새 값:** `SaveData`에 속성을 추가하고, 그 값을 가진 클래스가 `ISaveable`을 구현해 Root에 등록한다. 기존 세이브와 안 맞으면 버전을 올리고 `Migrate`에 변환을 넣는다.
- **새 씬:** `SceneId`에 값을 추가하고 `SceneLoader`의 이름 표에 씬 이름을 넣은 뒤 Build Settings에 씬을 등록한다. 씬마다 자기 LifetimeScope를 하나 두면 Root가 자동으로 부모가 된다.
- **새 공격 대상(바위 등):** `IDamageable`만 구현하면 `PlayerAttack`이 그대로 때린다.
- **새 나무 종류:** `Trees`를 상속하고 `TreeData`를 만든다. 특별한 반응은 `OnDamaged`/`FallAsync`를 재정의한다.
- **사운드:** `SoundId`에 값을 추가하고 `SoundLibrary` 에셋(`Data/Audio/`)에 파일을 연결한 뒤, `AudioManager`에서 해당 이벤트를 구독해 `PlaySfx`를 부른다.
- **늘 떠 있는 새 화면:** 뷰 인터페이스를 만들고 `UIManager`에서 연결한다. 단계에 딸린 화면은 그 상태 클래스에서 연다.

## 등록 규칙

- 씬을 넘어 유지할 것: `RootLifetimeScope`. 그 씬에서만 쓸 것: 씬의 LifetimeScope.
- 씬 오브젝트·뷰: `RegisterComponent(...).AsImplementedInterfaces()`
- 매 프레임 돌거나 시작 시 할 일이 있는 클래스: `RegisterEntryPoint<T>()` (`IInitializable`, `IStartable`, `ITickable`)
- 게임 단계: `Register<T>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf()`
- 그 밖의 서비스: `Register<T>(Lifetime.Singleton).AsImplementedInterfaces()`
