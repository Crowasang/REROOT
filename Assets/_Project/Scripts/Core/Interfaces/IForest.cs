using R3;

// 벌목할 나무들이 놓인 숲. 세션마다 새로 깔고, 쓰러진 나무와 다 베었는지를 알린다.
public interface IForest
{
    // 아직 다 쓰러지지 않은 나무 수
    int RemainingCount { get; }

    // 나무 한 그루가 다 쓰러졌을 때. 보상, 시간 보너스 등이 구독한다.
    Observable<Trees> OnTreeFelled { get; }

    // 마지막 나무가 쓰러졌을 때. OnTreeFelled 구독자가 모두 처리된 뒤에 발행된다.
    Observable<Unit> OnCleared { get; }

    // 기존 나무를 치우고 data의 나무로 숲을 새로 깐다.
    void Spawn(TreeData data);
}
