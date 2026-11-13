using System;
using Cysharp.Threading.Tasks;
using R3;
using VContainer.Unity;

// 세션이 끝날 때마다 저장한다. 세션 중에는 저장하지 않아서, 도중에 강제 종료하면 그 세션 획득분만 사라진다.
public sealed class SessionSaveTrigger : IStartable, IDisposable
{
    private readonly ISessionService _session;
    private readonly ISaveService _save;
    private IDisposable _subscription;

    public SessionSaveTrigger(ISessionService session, ISaveService save)
    {
        _session = session;
        _save = save;
    }

    public void Start()
    {
        _subscription = _session.OnEnded.Subscribe(_ => _save.SaveAsync().Forget());
    }

    public void Dispose()
    {
        _subscription?.Dispose();
    }
}
