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

    public StatService(UpgradeCatalog _catalog, IUpgradeService _upgrades)
    {
        this._catalog = _catalog;
        this._upgrades = _upgrades;

        Rebuild();
        _subscription = this._upgrades.OnLevelChanged.Subscribe(_ => Rebuild());
    }

    public Observable<Unit> OnChanged => _onChanged;

    public float Evaluate(StatType _stat, float _baseValue)
    {
        int _index = (int)_stat;
        return (_baseValue + _flat[_index]) * (1f + _percent[_index]);
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

        foreach (UpgradeData _upgrade in _catalog.Upgrades)
        {
            int _level = _upgrades.GetLevel(_upgrade);
            if (_level <= 0)
            {
                continue;
            }

            foreach (UpgradeEffect _effect in _upgrade.Effects)
            {
                float _amount = _effect.ValuePerLevel * _level;
                if (_effect.ModifierType == StatModifierType.Flat)
                {
                    _flat[(int)_effect.Stat] += _amount;
                }
                else
                {
                    _percent[(int)_effect.Stat] += _amount;
                }
            }
        }

        _onChanged.OnNext(Unit.Default);
    }
}
