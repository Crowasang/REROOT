// 공격으로 피해를 받을 수 있는 대상. 나무 말고 바위, 상자 같은 대상도 이걸 구현하면 공격이 그대로 통한다.
public interface IDamageable
{
    // 이미 쓰러졌거나 무적이면 false. 공격은 이 값이 true인 대상만 맞힌다.
    bool CanTakeDamage { get; }

    void TakeDamage(int damage);
}
