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

    public UpgradeService(UpgradeCatalog catalog, IWallet wallet)
    {
        _catalog = catalog;
        _wallet = wallet;
    }

    public Observable<UpgradeData> OnLevelChanged => _onLevelChanged;

    public int GetLevel(UpgradeData upgrade)
    {
        return upgrade != null && _levels.TryGetValue(upgrade.Id, out int level) ? level : 0;
    }

    public bool IsMaxLevel(UpgradeData upgrade)
    {
        return GetLevel(upgrade) >= upgrade.MaxLevel;
    }

    public int GetNextCost(UpgradeData upgrade)
    {
        return IsMaxLevel(upgrade) ? -1 : upgrade.GetCost(GetLevel(upgrade));
    }

    public bool CanPurchase(UpgradeData upgrade)
    {
        return upgrade != null && !IsMaxLevel(upgrade) && _wallet.Wood.CurrentValue >= GetNextCost(upgrade);
    }

    public bool TryPurchase(UpgradeData upgrade)
    {
        if (!CanPurchase(upgrade) || !_wallet.TrySpendWood(GetNextCost(upgrade)))
        {
            return false;
        }

        _levels[upgrade.Id] = GetLevel(upgrade) + 1;
        _onLevelChanged.OnNext(upgrade);
        return true;
    }

    public void Load(SaveData data)
    {
        _levels.Clear();
        foreach (KeyValuePair<string, int> pair in data.UpgradeLevels)
        {
            // 출시 후 지워진 강화가 세이브에 남아 있어도 무시한다.
            UpgradeData upgrade = _catalog.Find(pair.Key);
            if (upgrade != null)
            {
                _levels[pair.Key] = Math.Min(pair.Value, upgrade.MaxLevel);
            }
        }

        foreach (UpgradeData upgrade in _catalog.Upgrades)
        {
            _onLevelChanged.OnNext(upgrade);
        }
    }

    public void Write(SaveData data)
    {
        data.UpgradeLevels = new Dictionary<string, int>(_levels);
    }

    public void Dispose()
    {
        _onLevelChanged.Dispose();
    }
}
