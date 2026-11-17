using System;
using R3;
using UnityEngine;

// 실행 중 플레이어 능력치 = 기본값(PlayerStatsData) + 강화(IStatService).
// 강화를 사거나 세이브를 불러오면 IStatService가 알려 주고, 여기서 다시 계산한다.
// 이동·공격은 매번 여기서 현재 값을 읽으므로 강화가 바로 반영된다.
public sealed class PlayerStats : IDisposable
{
    private readonly PlayerStatsData _data;
    private readonly IStatService _stats;

    private readonly ReactiveProperty<float> _moveSpeed = new();
    private readonly ReactiveProperty<float> _turnSpeed = new();
    private readonly ReactiveProperty<int> _damage = new();
    private readonly ReactiveProperty<float> _attacksPerSecond = new();
    private readonly ReactiveProperty<float> _attackReach = new();
    private readonly ReactiveProperty<float> _attackRadius = new();
    private readonly ReactiveProperty<float> _attackHeight = new();
    private readonly IDisposable _subscription;

    public PlayerStats(PlayerStatsData data, IStatService stats)
    {
        _data = data;
        _stats = stats;

        Recalculate();
        _subscription = _stats.OnChanged.Subscribe(_ => Recalculate());
    }

    public ReadOnlyReactiveProperty<float> MoveSpeed => _moveSpeed;
    public ReadOnlyReactiveProperty<float> TurnSpeed => _turnSpeed;
    public ReadOnlyReactiveProperty<int> Damage => _damage;
    public ReadOnlyReactiveProperty<float> AttacksPerSecond => _attacksPerSecond;
    public ReadOnlyReactiveProperty<float> AttackReach => _attackReach;
    public ReadOnlyReactiveProperty<float> AttackRadius => _attackRadius;
    public ReadOnlyReactiveProperty<float> AttackHeight => _attackHeight;

    public void Dispose()
    {
        _subscription.Dispose();
        _moveSpeed.Dispose();
        _turnSpeed.Dispose();
        _damage.Dispose();
        _attacksPerSecond.Dispose();
        _attackReach.Dispose();
        _attackRadius.Dispose();
        _attackHeight.Dispose();
    }

    private void Recalculate()
    {
        _moveSpeed.Value = _stats.Evaluate(StatType.MoveSpeed, _data.MoveSpeed);
        _damage.Value = Mathf.Max(1, Mathf.RoundToInt(_stats.Evaluate(StatType.AttackDamage, _data.Damage)));
        _attacksPerSecond.Value = Mathf.Max(0.1f, _stats.Evaluate(StatType.AttacksPerSecond, _data.AttacksPerSecond));
        _attackReach.Value = _stats.Evaluate(StatType.AttackReach, _data.AttackReach);
        _attackRadius.Value = _stats.Evaluate(StatType.AttackRadius, _data.AttackRadius);

        // 회전 속도와 타격 높이는 강화 대상이 아니라 기본값 그대로 쓴다.
        _turnSpeed.Value = _data.TurnSpeed;
        _attackHeight.Value = _data.AttackHeight;
    }
}
