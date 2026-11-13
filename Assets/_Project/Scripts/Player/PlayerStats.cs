using System;
using R3;

// 실행 중 플레이어 능력치. 기본값(PlayerStatsData)으로 시작하고, 강화 시스템이 이 값을 올린다.
// 이동·공격은 매번 여기서 현재 값을 읽으므로 강화가 바로 반영된다.
public sealed class PlayerStats : IDisposable
{
    private readonly ReactiveProperty<float> _moveSpeed;
    private readonly ReactiveProperty<float> _turnSpeed;
    private readonly ReactiveProperty<int> _damage;
    private readonly ReactiveProperty<float> _attacksPerSecond;
    private readonly ReactiveProperty<float> _attackReach;
    private readonly ReactiveProperty<float> _attackRadius;
    private readonly ReactiveProperty<float> _attackHeight;

    public PlayerStats(PlayerStatsData data)
    {
        _moveSpeed = new ReactiveProperty<float>(data.MoveSpeed);
        _turnSpeed = new ReactiveProperty<float>(data.TurnSpeed);
        _damage = new ReactiveProperty<int>(data.Damage);
        _attacksPerSecond = new ReactiveProperty<float>(data.AttacksPerSecond);
        _attackReach = new ReactiveProperty<float>(data.AttackReach);
        _attackRadius = new ReactiveProperty<float>(data.AttackRadius);
        _attackHeight = new ReactiveProperty<float>(data.AttackHeight);
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
        _moveSpeed.Dispose();
        _turnSpeed.Dispose();
        _damage.Dispose();
        _attacksPerSecond.Dispose();
        _attackReach.Dispose();
        _attackRadius.Dispose();
        _attackHeight.Dispose();
    }
}
