using VContainer.Unity;

// 씬이 시작될 때 한 번: 맵 경계를 세우고, 카메라 범위를 정하고, 플레이어를 만든 뒤 게임 흐름을 숲 단계로 시작한다.
public sealed class GameInitializer : IStartable
{
    private readonly IMapBoundary _mapBoundary;
    private readonly ICameraController _camera;
    private readonly IPlayerSpawner _playerSpawner;
    private readonly IGameStateMachine _stateMachine;

    public GameInitializer(IMapBoundary mapBoundary, ICameraController camera, IPlayerSpawner playerSpawner, IGameStateMachine stateMachine)
    {
        _mapBoundary = mapBoundary;
        _camera = camera;
        _playerSpawner = playerSpawner;
        _stateMachine = stateMachine;
    }

    public void Start()
    {
        _mapBoundary.Build();
        _camera.SetBounds(_mapBoundary.CameraRect);
        _playerSpawner.Spawn();
        _stateMachine.ChangeState<ForestState>();
    }
}
