using R3;

// 숲에 머물 수 있는 남은 시간
public interface ISessionTimer
{
    // 이번 세션의 시작 시간. 시간 바의 100% 기준으로 쓴다.
    float Duration { get; }

    ReadOnlyReactiveProperty<float> Remaining { get; }

    // 시간이 다 돼서 끝났을 때만 발행한다. Stop()으로 멈출 때는 발행하지 않는다.
    Observable<Unit> OnTimeUp { get; }

    void Begin(float duration);

    void AddTime(float seconds);

    void Stop();
}
