using System;
using R3;

// 누적 기록. 기록을 더하는 쪽(게임 씬의 RecordManager)과 읽는 쪽(엔딩 등)이 서로를 모르게 여기 모은다.
public sealed class PlayerRecords : IPlayerRecords, ISaveable, IDisposable
{
    private readonly ReactiveProperty<int> _totalTreesFelled = new(0);
    private readonly ReactiveProperty<int> _totalSessions = new(0);

    public ReadOnlyReactiveProperty<int> TotalTreesFelled => _totalTreesFelled;
    public ReadOnlyReactiveProperty<int> TotalSessions => _totalSessions;

    public void AddTreeFelled()
    {
        _totalTreesFelled.Value++;
    }

    public void AddSession()
    {
        _totalSessions.Value++;
    }

    public void Load(SaveData data)
    {
        _totalTreesFelled.Value = data.TotalTreesFelled;
        _totalSessions.Value = data.TotalSessions;
    }

    public void Write(SaveData data)
    {
        data.TotalTreesFelled = _totalTreesFelled.Value;
        data.TotalSessions = _totalSessions.Value;
    }

    public void Dispose()
    {
        _totalTreesFelled.Dispose();
        _totalSessions.Dispose();
    }
}
