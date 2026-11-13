using System;
using R3;

// 플레이어가 가진 재화. 지금은 씬 안에서만 살지만, 세이브를 붙일 때 씬을 넘어 유지되는 계층으로 옮긴다.
public sealed class Wallet : IWallet, IDisposable
{
    private readonly ReactiveProperty<int> _wood = new(0);

    public ReadOnlyReactiveProperty<int> Wood => _wood;

    public void AddWood(int amount)
    {
        _wood.Value += amount;
    }

    public void Dispose()
    {
        _wood.Dispose();
    }
}
