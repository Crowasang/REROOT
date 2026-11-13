using System;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

// 일시정지만 맡는다. 세션 중에만 멈출 수 있고, 세션이 끝나면 자동으로 풀린다.
// 타이머, 공격 딜레이, 나무 흔들림이 모두 게임 시간을 쓰므로 Time.timeScale 하나로 함께 멈춘다.
public sealed class PauseManager : IPauseService, IDisposable
{
    private readonly ISessionService _session;
    private readonly InputAction _pauseAction;
    private readonly ReactiveProperty<bool> _isPaused = new(false);
    private readonly CompositeDisposable _disposables = new();

    // pauseAction: 일시정지 창을 열고 닫는 키. 플레이어 입력이 꺼진 일시정지 중에도 받아야 하므로 PlayerInput과 따로 둔다.
    public PauseManager(ISessionService session, InputAction pauseAction)
    {
        _session = session;
        _pauseAction = pauseAction;

        _pauseAction.performed += HandlePausePressed;
        _pauseAction.Enable();

        _session.IsActive
            .Where(isActive => !isActive)
            .Subscribe(_ => SetPaused(false))
            .AddTo(_disposables);
    }

    public ReadOnlyReactiveProperty<bool> IsPaused => _isPaused;

    public void Pause()
    {
        if (_session.IsActive.CurrentValue && !_isPaused.Value)
        {
            SetPaused(true);
        }
    }

    public void Resume()
    {
        if (_isPaused.Value)
        {
            SetPaused(false);
        }
    }

    public void Dispose()
    {
        _pauseAction.performed -= HandlePausePressed;
        _pauseAction.Disable();
        _disposables.Dispose();
        _isPaused.Dispose();

        // 일시정지 중에 씬이 내려가도 시간이 멈춘 채로 남지 않게 한다.
        Time.timeScale = 1f;
    }

    // ESC는 열려 있으면 닫고, 닫혀 있으면 연다.
    private void HandlePausePressed(InputAction.CallbackContext context)
    {
        if (_isPaused.Value)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    private void SetPaused(bool isPaused)
    {
        _isPaused.Value = isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }
}
