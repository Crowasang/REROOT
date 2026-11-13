using VContainer.Unity;

// 씬이 시작될 때 한 번: 맵 경계를 세우고, 카메라 범위를 정하고, 플레이어를 만들고, 첫 세션을 시작한다.
public sealed class GameInitializer : IStartable
{
    private readonly IMapBoundary _mapBoundary;
    private readonly ICameraController _camera;
    private readonly IPlayerSpawner _playerSpawner;
    private readonly ISessionService _session;

    public GameInitializer(IMapBoundary mapBoundary, ICameraController camera, IPlayerSpawner playerSpawner, ISessionService session)
    {
        _mapBoundary = mapBoundary;
        _camera = camera;
        _playerSpawner = playerSpawner;
        _session = session;
    }

    public void Start()
    {
        _mapBoundary.Build();
        _camera.SetBounds(_mapBoundary.CameraRect);
        _playerSpawner.Spawn();
        _session.StartSession();
    }
}
