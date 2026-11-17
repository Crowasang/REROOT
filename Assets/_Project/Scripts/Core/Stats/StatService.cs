using System;
using R3;

// 강화 단계에서 수치 수정자를 모아 최종값을 계산한다.
// 나중에 장비, 버프, 환생 보너스가 생기면 같은 방식으로 수정자를 더하는 곳만 늘리면 된다.
public sealed class StatService : IStatService, IDisposable
{
    private static readonly int STAT_COUNT = Enum.GetValues(typeof(StatType)).Length;

    private readonly UpgradeCatalog _catalog;
    private readonly IUpgradeService _upgrades;
    private readonly float[] _flat = new float[STAT_COUNT];
    private readonly float[] _percent = new float[STAT_COUNT];
    private readonly Subject<Unit> _onChanged = new();
    private readonly IDisposable _subscription;

    public StatService(UpgradeCatalog catalog, IUpgradeService upgrades)
    {
        _catalog = catalog;
        _upgrades = upgrades;

        Rebuild();
        _subscription = _upgrades.OnLevelChanged.Subscribe(_ => Rebuild());
    }

    public Observable<Unit> OnChanged => _onChanged;

    public float Evaluate(StatType stat, float baseValue)
    {
        int index = (int)stat;
        return (baseValue + _flat[index]) * (1f + _percent[index]);
    }

    public void Dispose()
    {
        _subscription.Dispose();
        _onChanged.Dispose();
    }

    // 강화가 수십 개라도 구매·불러오기 때만 다시 모으므로 매 프레임 계산 비용은 없다.
    private void Rebuild()
    {
        Array.Clear(_flat, 0, STAT_COUNT);
        Array.Clear(_percent, 0, STAT_COUNT);

        foreach (UpgradeData upgrade in _catalog.Upgrades)
        {
            int level = _upgrades.GetLevel(upgrade);
            if (level <= 0)
            {
                continue;
            }

            foreach (UpgradeEffect effect in upgrade.Effects)
            {
                float amount = effect.ValuePerLevel * level;
                if (effect.ModifierType == StatModifierType.Flat)
                {
                    _flat[(int)effect.Stat] += amount;
                }
                else
                {
                    _percent[(int)effect.Stat] += amount;
                }
            }
        }

        _onChanged.OnNext(Unit.Default);
    }
}
