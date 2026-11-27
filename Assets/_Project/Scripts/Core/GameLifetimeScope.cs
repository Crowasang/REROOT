using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

// 게임 씬의 조립 담당. 누가 무엇에 의존하는지는 여기 한 곳에만 적는다.
// 각 클래스는 생성자(또는 [Inject])로 필요한 인터페이스만 받고, 구체 클래스를 직접 찾지 않는다.
public class GameLifetimeScope : LifetimeScope
{
    [Header("데이터")]
    [SerializeField] private PlayerStatsData _playerStatsData;
    [SerializeField] private SessionSettings _sessionSettings;
    [SerializeField] private SoundLibrary _soundLibrary;

    [Header("씬 오브젝트")]
    [SerializeField] private TreeSpawner _treeSpawner;
    [SerializeField] private MapBoundary _mapBoundary;
    [SerializeField] private GameCamera _gameCamera;
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private AudioPlayer _audioPlayer;

    [Header("UI")]
    [SerializeField] private SessionHud _sessionHud;
    [SerializeField] private PausePanel _pausePanel;
    [SerializeField] private SessionEndPanel _sessionEndPanel;

    [Header("입력")]
    [Tooltip("일시정지 창을 열고 닫는 키. 플레이어 입력이 꺼진 일시정지 중에도 받아야 하므로 PlayerInput과 따로 둔다.")]
    [SerializeField] private InputAction _pauseAction = new("Pause", InputActionType.Button, "<Keyboard>/escape");

    protected override void Configure(IContainerBuilder _builder)
    {
        // 데이터
        _builder.RegisterInstance(_playerStatsData);
        _builder.RegisterInstance(_sessionSettings);
        _builder.RegisterInstance(_soundLibrary);

        // 상태 (재화·강화·기록·저장은 부모인 RootLifetimeScope에 있다)
        _builder.Register<PlayerStats>(Lifetime.Singleton);
        _builder.Register<SessionTimer>(Lifetime.Singleton).AsImplementedInterfaces();

        // 게임 흐름: 새 단계(강화, 로딩 등)는 상태 클래스를 만들어 여기 한 줄 추가한다.
        _builder.Register<GameStateMachine>(Lifetime.Singleton).AsImplementedInterfaces();
        _builder.Register<ForestState>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        _builder.Register<ResultState>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

        // 씬 오브젝트
        _builder.RegisterComponent(_treeSpawner).AsImplementedInterfaces();
        _builder.RegisterComponent(_playerSpawner).AsImplementedInterfaces();
        _builder.RegisterComponent(_mapBoundary).AsImplementedInterfaces();
        _builder.RegisterComponent(_gameCamera).AsImplementedInterfaces();
        _builder.RegisterComponent(_audioPlayer).AsImplementedInterfaces();

        // 뷰
        _builder.RegisterComponent(_sessionHud).AsImplementedInterfaces();
        _builder.RegisterComponent(_pausePanel).AsImplementedInterfaces();
        _builder.RegisterComponent(_sessionEndPanel).AsImplementedInterfaces();

        // 매니저
        _builder.Register<SessionManager>(Lifetime.Singleton).AsImplementedInterfaces();
        _builder.Register<PauseManager>(Lifetime.Singleton).AsImplementedInterfaces().WithParameter(_pauseAction);
        _builder.RegisterEntryPoint<RewardManager>();
        _builder.RegisterEntryPoint<PlayerControlManager>();
        _builder.RegisterEntryPoint<UIManager>();
        _builder.RegisterEntryPoint<AudioManager>();
        _builder.RegisterEntryPoint<RecordManager>();
        _builder.RegisterEntryPoint<SessionSaveTrigger>();
        _builder.RegisterEntryPoint<GameInitializer>();
    }
}
