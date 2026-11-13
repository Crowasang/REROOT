using System;
using R3;

// 세션 결과를 보여주는 단계. 계속하기를 누르면 다시 숲으로 간다.
// 강화 화면이 생기면 계속하기의 목적지만 UpgradeState로 바꾸면 된다.
public sealed class ResultState : IGameState
{
    private readonly ISessionService _session;
    private readonly ISessionEndView _view;
    private IDisposable _subscription;

    public ResultState(ISessionService session, ISessionEndView view)
    {
        _session = session;
        _view = view;
    }

    public void Enter(IGameStateMachine owner)
    {
        _view.Show(_session.LastResult);
        _subscription = _view.OnContinue.Take(1).Subscribe(_ => owner.ChangeState<ForestState>());
    }

    public void Exit()
    {
        _subscription?.Dispose();
        _subscription = null;
        _view.Hide();
    }

    public void Tick()
    {
    }
}
