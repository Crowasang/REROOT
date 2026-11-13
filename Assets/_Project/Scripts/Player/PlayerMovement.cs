using UnityEngine;
using UnityEngine.InputSystem;

// 바닥(XZ 평면) 위를 걷는다. 입력의 위쪽(W)이 화면 안쪽(+Z)이다.
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [Tooltip("바라보는 방향으로 몸을 돌리는 속도(도/초)")]
    [SerializeField] private float _turnSpeed = 900f;

    private Rigidbody _rb;
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
        _rb.MovePosition(_rb.position + _moveDirection * (_moveSpeed * Time.fixedDeltaTime));

        Quaternion targetRotation = Quaternion.LookRotation(_facingDirection, Vector3.up);
        _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, targetRotation, _turnSpeed * Time.fixedDeltaTime));
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

    // 입력을 끌 때 마지막 이동 입력이 남아 계속 미끄러지지 않도록 멈춘다.
    public void Stop()
    {
        _moveDirection = Vector3.zero;
        _rb.linearVelocity = Vector3.zero;
    }
}
