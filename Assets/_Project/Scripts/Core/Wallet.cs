using System;
using R3;

// 플레이어가 가진 재화. Root 계층에 있어 씬이 바뀌어도 유지되고, ISaveable로 저장된다.
public sealed class Wallet : IWallet, ISaveable, IDisposable
{
    private readonly ReactiveProperty<int> _wood = new(0);

    public ReadOnlyReactiveProperty<int> Wood => _wood;

    public void AddWood(int amount)
    {
        if (amount > 0)
        {
            _wood.Value += amount;
        }
    }

    public bool TrySpendWood(int amount)
    {
        if (amount < 0 || _wood.Value < amount)
        {
            return false;
        }

        _wood.Value -= amount;
        return true;
    }

    public void Load(SaveData data)
    {
        _wood.Value = Math.Max(0, data.Wood);
    }

    public void Write(SaveData data)
    {
        data.Wood = _wood.Value;
    }

    public void Dispose()
    {
        _wood.Dispose();
    }
}
