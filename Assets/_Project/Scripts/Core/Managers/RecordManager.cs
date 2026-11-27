using System;
using R3;
using VContainer.Unity;

// 게임 씬에서 일어나는 일을 누적 기록(Root의 IPlayerRecords)에 더한다.
public sealed class RecordManager : IStartable, IDisposable
{
    private readonly IPlayerRecords _records;
    private readonly IForest _forest;
    private readonly ISessionService _session;
    private readonly CompositeDisposable _disposables = new();

    public RecordManager(IPlayerRecords _records, IForest _forest, ISessionService _session)
    {
        this._records = _records;
        this._forest = _forest;
        this._session = _session;
    }

    public void Start()
    {
        _session.OnStarted.Subscribe(_ => _records.AddSession()).AddTo(_disposables);
        _forest.OnTreeFelled
            .Where(_ => _session.IsActive.CurrentValue)
            .Subscribe(_ => _records.AddTreeFelled())
            .AddTo(_disposables);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
