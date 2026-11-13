using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

// 바닥(XZ 평면) 위를 걷는다. 입력의 위쪽(W)이 화면 안쪽(+Z)이다.
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody _rb;
    private PlayerStats _stats;
    private Vector3 _moveDirection;
    private Vector3 _facingDirection = Vector3.back;

    // 마지막으로 움직인 방향(XZ, 길이 1). 멈춰 있어도 그 방향을 계속 바라본다.
    public Vector3 FacingDirection => _facingDirection;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_stats == null)
        {
            return;
        }

        // 강화로 속도가 바뀌어도 바로 반영되도록 매번 현재 값을 읽는다.
        _rb.MovePosition(_rb.position + _moveDirection * (_stats.MoveSpeed.CurrentValue * Time.fixedDeltaTime));

        Quaternion targetRotation = Quaternion.LookRotation(_facingDirection, Vector3.up);
        _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, targetRotation, _stats.TurnSpeed.CurrentValue * Time.fixedDeltaTime));
    }

    [Inject]
    public void Construct(PlayerStats stats)
    {
        _stats = stats;
    }

    // PlayerInput(Send Messages)이 Move 액션에서 호출한다.
    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        _moveDirection = new Vector3(input.x, 0f, input.y);

        if (_moveDirection.sqrMagnitude > 0.01f)
        {
            _facingDirection = _moveDirection.normalized;
        }
    }

    public void Stop()
    {
        _moveDirection = Vector3.zero;
        _rb.linearVelocity = Vector3.zero;
    }
}
