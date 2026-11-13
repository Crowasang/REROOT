# 구조

게임 씬은 VContainer로 조립한다. 조립 담당은 `GameLifetimeScope` 하나이고, 각 클래스는 생성자(또는 `[Inject]`)로 **인터페이스만** 받는다. 싱글톤과 `FindObjectOfType`은 쓰지 않는다.

## 매니저 (역할 하나씩)

| 매니저 | 맡는 일 | 의존 |
|---|---|---|
| `GameInitializer` | 씬 시작 시 한 번: 맵 경계, 카메라 범위, 플레이어 생성, 첫 세션 시작 | `IMapBoundary`, `ICameraController`, `IPlayerSpawner`, `ISessionService` |
| `SessionManager` (`ISessionService`) | 세션 시작과 끝, 결과 알림 | `IForest`, `ISessionTimer`, `IWallet`, `IPlayerSpawner`, `ICameraController` |
| `PauseManager` (`IPauseService`) | 일시정지, ESC, `Time.timeScale` | `ISessionService` |
| `RewardManager` | 나무 보상, 시간 보너스 | `IForest`, `IWallet`, `ISessionTimer`, `ISessionService` |
| `PlayerControlManager` | 플레이어 조작 켜고 끄기 (플레이어 있음 && 세션 중 && 안 멈춤) | `IPlayerSpawner`, `ISessionService`, `IPauseService` |
| `UIManager` | 뷰와 서비스 연결 (MVP의 프레젠터) | 뷰 인터페이스 + 서비스 |
| `AudioManager` | 게임에서 일어나는 일에 맞춰 어떤 소리를 낼지 결정 | `IAudioService`, `IPlayerSpawner`, `IForest`, `ISessionService`, `IPauseService`, 뷰 인터페이스 |

매니저끼리는 서로를 직접 호출하기보다 R3 이벤트(`OnStarted`, `OnEnded`, `IsActive`, `IsPaused`)를 구독해서 반응한다.

## 상태와 데이터

- **ScriptableObject (기본 수치, 런타임에 바꾸지 않음):** `PlayerStatsData`, `SessionSettings`, `TreeData` — `Assets/_Project/Data/`
- **실행 중 상태 (일반 C# 클래스):** `PlayerStats`(강화가 바꾸는 값), `Wallet`(`IWallet`), `SessionTimer`(`ISessionTimer`, `ITickable`)

## 씬 오브젝트 (MonoBehaviour)

- `TreeSpawner` (`IForest`): 숲 깔기·치우기. 자리 계산은 `ForestLayout`(순수 계산), 재사용은 `TreePool`.
- `Trees` (`IDamageable`): 나무 로직(체력, 판정, 쓰러짐 알림). 겉모습 연출은 `TreeView`.
- `PlayerSpawner` (`IPlayerSpawner`): 컨테이너로 플레이어를 만들어 `PlayerStats`를 주입한다.
- `PlayerController` (`IPlayerControl`): 바깥이 플레이어를 다루는 유일한 창구. `PlayerMovement`, `PlayerAttack`은 내부 구현.
- `GameCamera` (`ICameraController`), `MapBoundary` (`IMapBoundary`)
- `AudioPlayer` (`IAudioService`): 배경음 1채널과 효과음 여러 채널. 어떤 소리를 낼지는 모르고 요청받은 대로 튼다. 소리 파일과 볼륨은 `SoundLibrary`.
- 뷰: `SessionHud` (`IHudView`), `PausePanel` (`IPauseView`), `SessionEndPanel` (`ISessionEndView`) — 시스템을 모르고 표시와 클릭 알림만 한다.

## 새 기능을 붙이는 법

- **새 공격 대상(바위 등):** `IDamageable`만 구현하면 `PlayerAttack`이 그대로 때린다.
- **새 나무 종류:** `Trees`를 상속하고 `TreeData`를 만든다. 특별한 반응은 `OnDamaged`/`FallAsync`를 재정의한다.
- **강화:** `PlayerStats`에 값을 바꾸는 메서드를 추가하고, 강화 매니저가 `IWallet`과 `PlayerStats`를 받아 쓴다.
- **세이브·씬 전환:** 씬을 넘어 유지할 것(`Wallet`, 진행도, `SceneLoader`, `SaveService`)은 부모 `RootLifetimeScope`로 옮기고, `GameLifetimeScope`가 그걸 부모로 둔다. 인터페이스는 그대로라 사용하는 쪽 코드는 바뀌지 않는다.
- **사운드:** 새 소리는 `SoundId`에 값을 추가하고 `SoundLibrary` 에셋(`Data/Audio/`)에 파일을 연결한 뒤, `AudioManager`에서 해당 이벤트를 구독해 `PlaySfx`를 부른다. 소리를 실제로 내는 `AudioPlayer`(`IAudioService`)는 고칠 필요가 없다. 설정 화면의 볼륨 조절은 `IAudioService.SetSfxVolume/SetMusicVolume`을 쓴다.
- **새 화면:** 뷰 인터페이스를 만들고 `UIManager`에서 연결한다.

## 등록 규칙 (`GameLifetimeScope`)

- 씬 오브젝트·뷰: `RegisterComponent(...).AsImplementedInterfaces()`
- 매 프레임 돌거나 시작 시 할 일이 있는 클래스: `RegisterEntryPoint<T>()` (`IStartable`, `ITickable`)
- 그 밖의 서비스: `Register<T>(Lifetime.Singleton).AsImplementedInterfaces()`
