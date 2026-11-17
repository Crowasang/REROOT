using System;
using R3;

// 숲에서 나무를 베는 단계. 들어오면 세션을 시작하고, 세션이 끝나면 결과 단계로 넘긴다.
public sealed class ForestState : IGameState
{
    private readonly ISessionService _session;
    private IDisposable _subscription;

    public ForestState(ISessionService session)
    {
        _session = session;
    }

    public void Enter(IGameStateMachine owner)
    {
        // 세션이 끝나면(시간 종료·숲 정리·나가기) 다음 프레임이 아니라 바로 결과로 넘어간다.
        _subscription = _session.OnEnded.Take(1).Subscribe(_ => owner.ChangeState<ResultState>());
        _session.StartSession();
    }

    public void Exit()
    {
        _subscription?.Dispose();
        _subscription = null;
    }

    public void Tick()
    {
    }
}
