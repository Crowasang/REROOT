using System;
using R3;
using VContainer.Unity;

// 플레이어 조작을 켤지 끌지만 맡는다: 플레이어가 있고, 세션 중이고, 멈춰 있지 않을 때만 켠다.
// 세션·일시정지 매니저가 플레이어를 직접 건드리지 않아도 되게 하는 연결 고리다.
public sealed class PlayerControlManager : IStartable, IDisposable
{
    private readonly IPlayerSpawner _playerSpawner;
    private readonly ISessionService _session;
    private readonly IPauseService _pause;
    private readonly CompositeDisposable _disposables = new();

    public PlayerControlManager(IPlayerSpawner playerSpawner, ISessionService session, IPauseService pause)
    {
        _playerSpawner = playerSpawner;
        _session = session;
        _pause = pause;
    }

    public void Start()
    {
        Observable.CombineLatest(
                _playerSpawner.Player,
                _session.IsActive,
                _pause.IsPaused,
                (player, isActive, isPaused) => (player, canControl: isActive && !isPaused))
            .Subscribe(state => state.player?.SetControlEnabled(state.canControl))
            .AddTo(_disposables);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
