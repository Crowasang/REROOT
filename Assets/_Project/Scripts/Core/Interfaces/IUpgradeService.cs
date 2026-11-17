using R3;

// 강화 단계를 들고 있고, 구매를 처리한다. 저장 대상이다.
public interface IUpgradeService
{
    // 어떤 강화의 단계가 올랐을 때(구매, 불러오기)
    Observable<UpgradeData> OnLevelChanged { get; }

    int GetLevel(UpgradeData upgrade);

    bool IsMaxLevel(UpgradeData upgrade);

    // 다음 단계 비용. 최대 단계면 -1.
    int GetNextCost(UpgradeData upgrade);

    bool CanPurchase(UpgradeData upgrade);

    // 재화가 모자라거나 최대 단계면 false를 돌려주고 아무것도 바꾸지 않는다.
    bool TryPurchase(UpgradeData upgrade);
}
