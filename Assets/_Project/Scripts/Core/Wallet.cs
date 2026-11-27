using System;
using R3;

// 플레이어가 가진 재화. Root 계층에 있어 씬이 바뀌어도 유지되고, ISaveable로 저장된다.
public sealed class Wallet : IWallet, ISaveable, IDisposable
{
    private readonly ReactiveProperty<int> _wood = new(0);

    public ReadOnlyReactiveProperty<int> Wood => _wood;

    public void AddWood(int _amount)
    {
        if (_amount > 0)
        {
            _wood.Value += _amount;
        }
    }

    public bool TrySpendWood(int _amount)
    {
        if (_amount < 0 || _wood.Value < _amount)
        {
            return false;
        }

        _wood.Value -= _amount;
        return true;
    }

    public void Load(SaveData _data)
    {
        _wood.Value = Math.Max(0, _data.Wood);
    }

    public void Write(SaveData _data)
    {
        _data.Wood = _wood.Value;
    }

    public void Dispose()
    {
        _wood.Dispose();
    }
}
