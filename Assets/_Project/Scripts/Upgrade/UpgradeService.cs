using System;
using System.Collections.Generic;
using R3;

// 강화 단계 보관과 구매. 재화는 IWallet에서 쓰고, 단계가 바뀌면 알린다.
// 저장은 직접 하지 않고 ISaveable로 세이브 시스템에 맡긴다(저장 시점은 AutoSaveManager가 정한다).
public sealed class UpgradeService : IUpgradeService, ISaveable, IDisposable
{
    private readonly UpgradeCatalog _catalog;
    private readonly IWallet _wallet;
    private readonly Dictionary<string, int> _levels = new();
    private readonly Subject<UpgradeData> _onLevelChanged = new();

    public UpgradeService(UpgradeCatalog _catalog, IWallet _wallet)
    {
        this._catalog = _catalog;
        this._wallet = _wallet;
    }

    public Observable<UpgradeData> OnLevelChanged => _onLevelChanged;

    public int GetLevel(UpgradeData _upgrade)
    {
        return _upgrade != null && _levels.TryGetValue(_upgrade.Id, out int _level) ? _level : 0;
    }

    public bool IsMaxLevel(UpgradeData _upgrade)
    {
        return GetLevel(_upgrade) >= _upgrade.MaxLevel;
    }

    public int GetNextCost(UpgradeData _upgrade)
    {
        return IsMaxLevel(_upgrade) ? -1 : _upgrade.GetCost(GetLevel(_upgrade));
    }

    public bool CanPurchase(UpgradeData _upgrade)
    {
        return _upgrade != null && !IsMaxLevel(_upgrade) && _wallet.Wood.CurrentValue >= GetNextCost(_upgrade);
    }

    public bool TryPurchase(UpgradeData _upgrade)
    {
        if (!CanPurchase(_upgrade) || !_wallet.TrySpendWood(GetNextCost(_upgrade)))
        {
            return false;
        }

        _levels[_upgrade.Id] = GetLevel(_upgrade) + 1;
        _onLevelChanged.OnNext(_upgrade);
        return true;
    }

    public void Load(SaveData _data)
    {
        _levels.Clear();
        foreach (KeyValuePair<string, int> _pair in _data.UpgradeLevels)
        {
            // 출시 후 지워진 강화가 세이브에 남아 있어도 무시한다.
            UpgradeData _upgrade = _catalog.Find(_pair.Key);
            if (_upgrade != null)
            {
                _levels[_pair.Key] = Math.Min(_pair.Value, _upgrade.MaxLevel);
            }
        }

        foreach (UpgradeData _upgrade in _catalog.Upgrades)
        {
            _onLevelChanged.OnNext(_upgrade);
        }
    }

    public void Write(SaveData _data)
    {
        _data.UpgradeLevels = new Dictionary<string, int>(_levels);
    }

    public void Dispose()
    {
        _onLevelChanged.Dispose();
    }
}
