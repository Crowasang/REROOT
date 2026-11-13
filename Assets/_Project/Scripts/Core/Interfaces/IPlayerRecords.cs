using R3;

// 누적 기록(벤 나무 수, 숲에 들어간 횟수). 엔딩·업적·통계 화면이 읽는다. 저장 대상이다.
public interface IPlayerRecords
{
    ReadOnlyReactiveProperty<int> TotalTreesFelled { get; }
    ReadOnlyReactiveProperty<int> TotalSessions { get; }

    void AddTreeFelled();

    void AddSession();
}
