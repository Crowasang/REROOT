using R3;

// 플레이어가 가진 재화. 씬을 넘어 유지되고 저장된다.
public interface IWallet
{
    ReadOnlyReactiveProperty<int> Wood { get; }

    void AddWood(int amount);

    // 모자라면 false를 돌려주고 아무것도 바꾸지 않는다.
    bool TrySpendWood(int amount);
}
