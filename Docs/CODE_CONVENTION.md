# 코드 컨벤션

## 이름 규칙

| 대상 | 규칙 | 예시 |
|---|---|---|
| 클래스, 구조체, 열거형, 인터페이스 | PascalCase (인터페이스는 `I` 접두사) | `TreeSpawner`, `IDamageable` |
| 메서드, 프로퍼티, 이벤트 | PascalCase | `TakeDamage()`, `MaxHp` |
| 필드 (private, protected, `[SerializeField]`) | `_camelCase` | `_moveSpeed`, `_currentHp` |
| 지역 변수, 매개변수 | camelCase | `spawnPosition`, `damage` |
| 상수 (`const`, `static readonly` 상수 값) | UPPER_SNAKE_CASE | `MAX_STAGE`, `DEFAULT_SESSION_TIME` |
| 열거형 값 | PascalCase | `TreeState.Falling` |
| 비동기 메서드 | `Async` 접미사 | `PlayFallAsync()` |

- 외부에 값을 보여줄 때는 public 필드 대신 프로퍼티를 쓴다.
  ```csharp
  [SerializeField] private int _maxHp = 10;
  public int MaxHp => _maxHp;
  ```
- 약어도 단어로 취급한다: `UiPanel`, `HpBar` (`UIPanel`, `HPBar` 아님).
- 철자를 확인한다. 이름은 나중에 바꾸기 번거롭다.

## 파일과 폴더

- 파일 하나에 최상위 타입 하나. 파일 이름은 타입 이름과 같게 한다.
- 우리 프로젝트 에셋은 모두 `Assets/_Project/` 아래에 둔다. 외부 에셋과 패키지는 그 밖에 둔다.

```
Assets/_Project/
├─ Scripts/
│  ├─ Core/       게임 흐름, 매니저, 공용 유틸
│  ├─ Player/     플레이어 이동, 벌목
│  ├─ Tree/       나무, 나무 스폰
│  ├─ Upgrade/    강화 시스템
│  ├─ UI/         UI 패널, HUD
│  └─ Data/       ScriptableObject 정의
├─ Data/          ScriptableObject 에셋 (Trees/, Upgrades/ ...)
├─ Prefabs/       Player/, Trees/, UI/ ...
├─ Scenes/
├─ Input/
├─ Art/           Models/, Materials/, Textures/
└─ Audio/
```

- 데이터 에셋 이름: `종류_번호_이름` (예: `Tree_01_Birch`, `Tree_02_Pine`).
- 파일과 폴더를 옮길 때는 Unity 에디터 안에서 옮기거나, 탐색기/git으로 옮길 때 `.meta` 파일을 같이 옮긴다. `.meta`가 빠지면 참조가 끊긴다.

## 클래스 내부 순서

1. 상수
2. `[SerializeField]` 필드
3. private 필드
4. 프로퍼티, 이벤트(Observable)
5. Unity 메시지 (`Awake`, `OnEnable`, `Start`, `Update`, `OnDisable`, `OnDestroy` 순)
6. public 메서드
7. private 메서드

## 수치와 데이터

- 밸런스 수치(체력, 보상, 비용, 배율)는 코드에 직접 쓰지 않고 ScriptableObject에 둔다.
- ScriptableObject의 값은 런타임에 바꾸지 않는다. 바뀌는 값(현재 체력 등)은 컴포넌트가 따로 들고 있는다.

## UniTask

- 코루틴 대신 UniTask를 쓴다.
- 비동기 메서드는 `CancellationToken`을 마지막 매개변수로 받는다. MonoBehaviour에서는 `destroyCancellationToken`을 넘긴다.
  ```csharp
  private async UniTask PlayFallAsync(CancellationToken token)
  {
      await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);
  }
  ```
- 결과를 기다리지 않는 호출은 `UniTaskVoid`와 `.Forget()`을 쓴다. `async void`는 쓰지 않는다.

## R3

- 값 변화를 알릴 때는 `ReactiveProperty`, 일회성 이벤트는 `Subject`를 쓴다.
- 밖에는 읽기 전용으로만 노출한다.
  ```csharp
  private readonly ReactiveProperty<int> _wood = new(0);
  public ReadOnlyReactiveProperty<int> Wood => _wood;

  private readonly Subject<Trees> _onTreeFelled = new();
  public Observable<Trees> OnTreeFelled => _onTreeFelled;
  ```
- 구독은 반드시 `.AddTo(this)`로 수명을 묶는다. 직접 만든 `ReactiveProperty`, `Subject`는 `OnDestroy`에서 `Dispose()` 한다.
- 시스템끼리 직접 참조하는 대신 Observable로 연결한다. (예: 나무가 쓰러지면 재화, UI, 패시브가 각각 구독해서 반응)
