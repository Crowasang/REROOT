using System;
using R3;
using UnityEngine;
using VContainer.Unity;

// 늘 떠 있는 화면(HUD, 일시정지 창)과 시스템을 잇는 일만 맡는다.
// 게임 단계에 딸린 화면(결과, 강화 등)은 그 단계의 상태 클래스가 직접 열고 닫는다.
public sealed class UIManager : IStartable, IDisposable
{
    private readonly IHudView _hud;
    private readonly IPauseView _pauseView;
    private readonly ISessionService _session;
    private readonly IPauseService _pause;
    private readonly ISessionTimer _timer;
    private readonly IWallet _wallet;
    private readonly CompositeDisposable _disposables = new();

    public UIManager(
        IHudView hud,
        IPauseView pauseView,
        ISessionService session,
        IPauseService pause,
        ISessionTimer timer,
        IWallet wallet)
    {
        _hud = hud;
        _pauseView = pauseView;
        _session = session;
        _pause = pause;
        _timer = timer;
        _wallet = wallet;
    }

    public void Start()
    {
        _pauseView.Hide();

        BindHud();
        BindPause();
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
}
