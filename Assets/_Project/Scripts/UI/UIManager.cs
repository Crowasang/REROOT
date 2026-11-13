using System;
using R3;
using UnityEngine;
using VContainer.Unity;

// 화면(뷰)과 시스템(서비스)을 잇는 일만 맡는다.
// 뷰는 시스템을 모르고, 시스템은 화면을 모른다. 화면을 바꾸거나 새 패널을 붙일 때는 여기만 고치면 된다.
public sealed class UIManager : IStartable, IDisposable
{
    private readonly IHudView _hud;
    private readonly IPauseView _pauseView;
    private readonly ISessionEndView _sessionEndView;
    private readonly ISessionService _session;
    private readonly IPauseService _pause;
    private readonly ISessionTimer _timer;
    private readonly IWallet _wallet;
    private readonly CompositeDisposable _disposables = new();

    public UIManager(
        IHudView hud,
        IPauseView pauseView,
        ISessionEndView sessionEndView,
        ISessionService session,
        IPauseService pause,
        ISessionTimer timer,
        IWallet wallet)
    {
        _hud = hud;
        _pauseView = pauseView;
        _sessionEndView = sessionEndView;
        _session = session;
        _pause = pause;
        _timer = timer;
        _wallet = wallet;
    }

    public void Start()
    {
        _pauseView.Hide();
        _sessionEndView.Hide();

        BindHud();
        BindPause();
        BindSessionEnd();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }

    private void BindHud()
    {
        _timer.Remaining
            .Subscribe(remaining => _hud.SetTime(
                _timer.Duration > 0f ? remaining / _timer.Duration : 0f,
                Mathf.CeilToInt(remaining)))
            .AddTo(_disposables);

        _wallet.Wood.Subscribe(_hud.SetWood).AddTo(_disposables);
        _hud.OnPauseClicked.Subscribe(_ => _pause.Pause()).AddTo(_disposables);
    }

    private void BindPause()
    {
        _pause.IsPaused
            .Subscribe(isPaused =>
            {
                if (isPaused)
                {
                    _pauseView.Show();
                }
                else
                {
                    _pauseView.Hide();
                }
            })
            .AddTo(_disposables);

        _pauseView.OnResume.Subscribe(_ => _pause.Resume()).AddTo(_disposables);
        _pauseView.OnLeave.Subscribe(_ => _session.EndSession(SessionEndReason.Left)).AddTo(_disposables);
    }

    private void BindSessionEnd()
    {
        _session.OnStarted.Subscribe(_ => _sessionEndView.Hide()).AddTo(_disposables);
        _session.OnEnded.Subscribe(_sessionEndView.Show).AddTo(_disposables);
        _sessionEndView.OnContinue.Subscribe(_ => _session.StartSession()).AddTo(_disposables);
    }
}
