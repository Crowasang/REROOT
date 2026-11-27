using System;
using R3;
using UnityEngine;

// 숲에 머물 수 있는 남은 시간. 화면에는 플레이어 체력 바처럼 보여준다.
// 도는 동안에만 프레임마다 시간을 줄인다(R3 EveryUpdate 구독). 멈추면 구독을 끊는다.
public sealed class SessionTimer : ISessionTimer, IDisposable
{
    private readonly ReactiveProperty<float> _remaining = new(0f);
    private readonly Subject<Unit> _onTimeUp = new();
    private IDisposable _countdown;
    private float _duration;

    public float Duration => _duration;
    public ReadOnlyReactiveProperty<float> Remaining => _remaining;
    public Observable<Unit> OnTimeUp => _onTimeUp;
    private bool IsRunning => _countdown != null;

    public void Begin(float _duration)
    {
        this._duration = _duration;
        _remaining.Value = _duration;
        _countdown?.Dispose();
        _countdown = Observable.EveryUpdate().Subscribe(_ => CountDown());
    }

    public void AddTime(float _seconds)
    {
        if (IsRunning)
        {
            _remaining.Value += _seconds;
        }
    }

    public void Stop()
    {
        _countdown?.Dispose();
        _countdown = null;
    }

    public void Dispose()
    {
        _countdown?.Dispose();
        _remaining.Dispose();
        _onTimeUp.Dispose();
    }

    private void CountDown()
    {
        // Time.timeScale이 0이면 deltaTime도 0이라 일시정지 중에는 시간이 줄지 않는다.
        _remaining.Value = Mathf.Max(0f, _remaining.Value - Time.deltaTime);
        if (_remaining.Value <= 0f)
        {
            Stop();
            _onTimeUp.OnNext(Unit.Default);
        }
    }
}
