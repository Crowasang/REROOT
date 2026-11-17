using R3;
using UnityEngine;

// 바깥(매니저)에서 플레이어에게 할 수 있는 일과 플레이어가 알리는 일. 이동·공격 구현은 몰라도 된다.
public interface IPlayerControl
{
    Transform Transform { get; }

    // 휘두를 때마다. 효과음, 애니메이션이 구독한다.
    Observable<Unit> OnSwing { get; }

    // 끄면 입력을 막고 그 자리에 멈춘다.
    void SetControlEnabled(bool isEnabled);
}
