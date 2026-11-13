using UnityEngine;

// 바깥(매니저)에서 플레이어에게 할 수 있는 일. 이동·공격 구현은 몰라도 된다.
public interface IPlayerControl
{
    Transform Transform { get; }

    // 끄면 입력을 막고 그 자리에 멈춘다.
    void SetControlEnabled(bool isEnabled);
}
