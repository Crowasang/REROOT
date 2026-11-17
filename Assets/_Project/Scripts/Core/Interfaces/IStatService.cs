using R3;

// 기본값에 강화 등으로 생긴 수정자를 적용해 최종값을 돌려준다.
// 수치를 쓰는 쪽은 강화가 몇 개, 어떤 종류인지 몰라도 된다.
public interface IStatService
{
    // 수정자가 바뀌었을 때(강화 구매, 세이브 불러오기). 최종값을 다시 계산해야 하는 쪽이 구독한다.
    Observable<Unit> OnChanged { get; }

    float Evaluate(StatType stat, float baseValue);
}
