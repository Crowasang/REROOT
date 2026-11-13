using System;
using R3;

// 세션(숲에 들어가서 나올 때까지) 흐름만 맡는다.
// 시작: 숲을 깔고, 플레이어를 시작 자리로, 시간을 처음부터. 끝: 시간을 멈추고 결과를 알린다.
// 화면, 일시정지, 보상, 플레이어 조작은 다른 매니저가 이 매니저의 이벤트를 구독해서 처리한다.
public sealed class SessionManager : ISessionService, IDisposable
{
    private readonly IForest _forest;
    private readonly ISessionTimer _timer;
    private readonly IWallet _wallet;
    private readonly IPlayerSpawner _playerSpawner;
    private readonly ICameraController _camera;
    private readonly IStatService _stats;
    private readonly SessionSettings _settings;

    private readonly ReactiveProperty<bool> _isActive = new(false);
    private readonly Subject<Unit> _onStarted = new();
    private readonly Subject<SessionResult> _onEnded = new();
    private readonly CompositeDisposable _disposables = new();
    private int _woodAtStart;
    private SessionResult _lastResult;

    public SessionManager(
        IForest forest,
        ISessionTimer timer,
        IWallet wallet,
        IPlayerSpawner playerSpawner,
        ICameraController camera,
        IStatService stats,
        SessionSettings settings)
    {
        _forest = forest;
        _timer = timer;
        _wallet = wallet;
        _playerSpawner = playerSpawner;
        _camera = camera;
        _stats = stats;
        _settings = settings;

        _timer.OnTimeUp.Subscribe(_ => EndSession(SessionEndReason.TimeUp)).AddTo(_disposables);
        _forest.OnCleared.Subscribe(_ => EndSession(SessionEndReason.Cleared)).AddTo(_disposables);
    }

    public ReadOnlyReactiveProperty<bool> IsActive => _isActive;
    public Observable<Unit> OnStarted => _onStarted;
    public Observable<SessionResult> OnEnded => _onEnded;
    public SessionResult LastResult => _lastResult;

    public void StartSession()
    {
        _forest.Spawn(_settings.StartTree);

        _playerSpawner.MoveToSpawnPoint();
        IPlayerControl player = _playerSpawner.Player.CurrentValue;
        if (player != null)
        {
            _camera.SetTarget(player.Transform);
        }

        _woodAtStart = _wallet.Wood.CurrentValue;
        // "숲에 머무는 시간 +5초" 같은 강화가 반영되도록 스탯을 거친다.
        _timer.Begin(_stats.Evaluate(StatType.SessionDuration, _settings.BaseDuration));
        _isActive.Value = true;
        _onStarted.OnNext(Unit.Default);
    }

    public void EndSession(SessionEndReason reason)
    {
        if (!_isActive.Value)
        {
            return;
        }

        _timer.Stop();
        _isActive.Value = false;
        _lastResult = new SessionResult(reason, _wallet.Wood.CurrentValue - _woodAtStart);
        _onEnded.OnNext(_lastResult);
    }

    public void Dispose()
    {
        _disposables.Dispose();
        _isActive.Dispose();
        _onStarted.Dispose();
        _onEnded.Dispose();
    }
}
