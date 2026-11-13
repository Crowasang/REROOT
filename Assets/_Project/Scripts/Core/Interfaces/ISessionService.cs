using R3;

// 숲에 들어가서 나올 때까지의 한 판(세션) 흐름
public interface ISessionService
{
    ReadOnlyReactiveProperty<bool> IsActive { get; }

    Observable<Unit> OnStarted { get; }

    Observable<SessionResult> OnEnded { get; }

    // 가장 최근에 끝난 세션의 결과. 결과 화면이 읽는다.
    SessionResult LastResult { get; }

    void StartSession();

    // 이미 끝난 세션이면 아무것도 하지 않는다.
    void EndSession(SessionEndReason reason);
}
