// 강화로 바뀌는 수치의 종류. 새 수치를 강화하고 싶으면 값을 추가하고, 그 수치를 쓰는 곳에서
// IStatService.Evaluate(StatType.새값, 기본값)으로 최종값을 받으면 된다.
public enum StatType
{
    MoveSpeed,
    AttackDamage,
    AttacksPerSecond,
    AttackRadius,
    AttackReach,
    SessionDuration,
    TreeBonusTime,
    WoodGain,
}
