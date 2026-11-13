using R3;

// 플레이어가 가진 재화. 강화 구매와 저장이 여기서 값을 읽고 쓴다.
public interface IWallet
{
    ReadOnlyReactiveProperty<int> Wood { get; }

    void AddWood(int amount);
}
