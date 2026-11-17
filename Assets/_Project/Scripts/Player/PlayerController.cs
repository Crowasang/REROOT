using R3;
using UnityEngine;
using UnityEngine.InputSystem;

// 바깥(매니저)이 플레이어를 다루는 창구. 입력·이동·공격 컴포넌트를 직접 건드리지 않고 이 인터페이스만 쓴다.
[RequireComponent(typeof(PlayerInput), typeof(PlayerMovement), typeof(PlayerAttack))]
public class PlayerController : MonoBehaviour, IPlayerControl
{
    private PlayerInput _playerInput;
    private PlayerMovement _movement;
    private PlayerAttack _attack;

    public Transform Transform => transform;
    public Observable<Unit> OnSwing => _attack.OnSwing;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _movement = GetComponent<PlayerMovement>();
        _attack = GetComponent<PlayerAttack>();
    }

    public void SetControlEnabled(bool isEnabled)
    {
        if (isEnabled)
        {
            _playerInput.ActivateInput();
            return;
        }

        _playerInput.DeactivateInput();

        // 입력을 끌 때 마지막 이동 입력이 남아 계속 미끄러지지 않도록 멈춘다.
        _movement.Stop();
    }
}
