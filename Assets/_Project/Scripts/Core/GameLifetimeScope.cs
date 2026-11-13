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

    protected override void Configure(IContainerBuilder builder)
    {
        // 데이터
        builder.RegisterInstance(_playerStatsData);
        builder.RegisterInstance(_sessionSettings);
        builder.RegisterInstance(_soundLibrary);

        // 상태
        builder.Register<PlayerStats>(Lifetime.Singleton);
        builder.Register<Wallet>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.RegisterEntryPoint<SessionTimer>().AsSelf();

        // 씬 오브젝트
        builder.RegisterComponent(_treeSpawner).AsImplementedInterfaces();
        builder.RegisterComponent(_playerSpawner).AsImplementedInterfaces();
        builder.RegisterComponent(_mapBoundary).AsImplementedInterfaces();
        builder.RegisterComponent(_gameCamera).AsImplementedInterfaces();
        builder.RegisterComponent(_audioPlayer).AsImplementedInterfaces();

        // 뷰
        builder.RegisterComponent(_sessionHud).AsImplementedInterfaces();
        builder.RegisterComponent(_pausePanel).AsImplementedInterfaces();
        builder.RegisterComponent(_sessionEndPanel).AsImplementedInterfaces();

        // 매니저
        builder.Register<SessionManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<PauseManager>(Lifetime.Singleton).AsImplementedInterfaces().WithParameter(_pauseAction);
        builder.RegisterEntryPoint<RewardManager>();
        builder.RegisterEntryPoint<PlayerControlManager>();
        builder.RegisterEntryPoint<UIManager>();
        builder.RegisterEntryPoint<AudioManager>();
        builder.RegisterEntryPoint<GameInitializer>();
    }
}
