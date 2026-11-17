using System;
using R3;
using VContainer.Unity;

// 게임에서 일어나는 일에 맞춰 어떤 소리를 낼지만 정한다. 소리를 실제로 내는 건 IAudioService.
// 새 소리를 붙일 때는 이벤트를 하나 더 구독하면 되고, 다른 시스템 코드는 건드리지 않는다.
public sealed class AudioManager : IStartable, IDisposable
{
    private readonly IAudioService _audio;
    private readonly IPlayerSpawner _playerSpawner;
    private readonly IForest _forest;
    private readonly ISessionService _session;
    private readonly IPauseService _pause;
    private readonly IHudView _hud;
    private readonly IPauseView _pauseView;
    private readonly ISessionEndView _sessionEndView;
    private readonly CompositeDisposable _disposables = new();

    public AudioManager(
        IAudioService audio,
        IPlayerSpawner playerSpawner,
        IForest forest,
        ISessionService session,
        IPauseService pause,
        IHudView hud,
        IPauseView pauseView,
        ISessionEndView sessionEndView)
    {
        _audio = audio;
        _playerSpawner = playerSpawner;
        _forest = forest;
        _session = session;
        _pause = pause;
        _hud = hud;
        _pauseView = pauseView;
        _sessionEndView = sessionEndView;
    }

    public void Start()
    {
        BindGameplay();
        BindSession();
        BindUi();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }

    private void BindGameplay()
    {
        // 플레이어는 나중에 만들어지므로, 만들어질 때마다 그 플레이어의 휘두르기를 구독한다.
        _playerSpawner.Player
            .Where(player => player != null)
            .Select(player => player.OnSwing)
            .Switch()
            .Subscribe(_ => _audio.PlaySfx(SoundId.Swing))
            .AddTo(_disposables);

        _forest.OnTreeHit
            .Subscribe(tree => _audio.PlaySfx(tree.IsFelled ? SoundId.TreeFall : SoundId.Chop))
            .AddTo(_disposables);

        // 쓰러지는 연출이 끝나고 재화가 들어오는 순간
        _forest.OnTreeFelled
            .Where(_ => _session.IsActive.CurrentValue)
            .Subscribe(_ => _audio.PlaySfx(SoundId.Reward))
            .AddTo(_disposables);
    }

    private void BindSession()
    {
        _session.OnStarted.Subscribe(_ => _audio.PlayMusic(MusicId.Forest)).AddTo(_disposables);

        _session.OnEnded
            .Subscribe(result =>
            {
                switch (result.Reason)
                {
                    case SessionEndReason.TimeUp:
                        _audio.PlaySfx(SoundId.TimeUp);
                        break;
                    case SessionEndReason.Cleared:
                        _audio.PlaySfx(SoundId.Cleared);
                        break;
                }
            })
            .AddTo(_disposables);

        _pause.IsPaused.Subscribe(_audio.SetMusicDucked).AddTo(_disposables);
    }

    private void BindUi()
    {
        Observable.Merge(_hud.OnPauseClicked, _pauseView.OnResume, _pauseView.OnLeave, _sessionEndView.OnContinue)
            .Subscribe(_ => _audio.PlaySfx(SoundId.UiClick))
            .AddTo(_disposables);
    }
}
