using System;
using R3;
using UnityEngine;
using VContainer.Unity;

// 숲에 머물 수 있는 남은 시간. 화면에는 플레이어 체력 바처럼 보여준다.
// 컨테이너가 매 프레임 Tick을 불러 준다(ITickable).
public sealed class SessionTimer : ISessionTimer, ITickable, IDisposable
{
    private readonly ReactiveProperty<float> _remaining = new(0f);
    private readonly Subject<Unit> _onTimeUp = new();
    private bool _isRunning;
    private float _duration;

    public float Duration => _duration;
    public ReadOnlyReactiveProperty<float> Remaining => _remaining;
    public Observable<Unit> OnTimeUp => _onTimeUp;

    public void Tick()
    {
        if (!_isRunning)
        {
            return;
        }

        // Time.timeScale이 0이면 deltaTime도 0이라 일시정지 중에는 시간이 줄지 않는다.
        _remaining.Value = Mathf.Max(0f, _remaining.Value - Time.deltaTime);
        if (_remaining.Value <= 0f)
        {
            _isRunning = false;
            _onTimeUp.OnNext(Unit.Default);
        }
    }

    public void Begin(float duration)
    {
        _duration = duration;
        _remaining.Value = duration;
        _isRunning = true;
    }

    public void AddTime(float seconds)
    {
        if (_isRunning)
        {
            _remaining.Value += seconds;
        }
    }

    public void Stop()
    {
        _isRunning = false;
    }

    public void Dispose()
    {
        _remaining.Dispose();
        _onTimeUp.Dispose();
    }
}
