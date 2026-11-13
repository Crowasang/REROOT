// 강화가 수치를 바꾸는 방식. 최종값 = (기본값 + Flat 합) × (1 + Percent 합)
public enum StatModifierType
{
    // 더하기. 예: 공격력 +1
    Flat,

    // 비율. 0.1 = +10%. 같은 수치의 Percent끼리는 더한 뒤 한 번에 곱한다.
    Percent,
}
