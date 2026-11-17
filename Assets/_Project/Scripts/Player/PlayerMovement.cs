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

    // 실제로 움직이는 빠르기를 최고 속도 대비 0~1로. 나무에 막혀 있으면 입력을 넣어도 0에 가깝다.
    // 달리기 애니메이션이 이 값을 쓴다.
    public float NormalizedSpeed
    {
        get
        {
            if (_stats == null || _stats.MoveSpeed.CurrentValue <= 0f)
            {
                return 0f;
            }

            Vector3 velocity = _rb.linearVelocity;
            velocity.y = 0f;
            return Mathf.Clamp01(velocity.magnitude / _stats.MoveSpeed.CurrentValue);
        }
    }

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

        // 위치를 직접 옮기면(MovePosition) 나무 틈에 대고 밀 때 겹침을 풀면서 반대편으로 빠져나갈 수 있다.
        // 속도를 주면 물리 엔진이 충돌을 먼저 처리하므로 좁은 틈을 비집고 지나가지 못한다.
        // 강화로 속도가 바뀌어도 바로 반영되도록 매번 현재 값을 읽는다.
        _rb.linearVelocity = _moveDirection * _stats.MoveSpeed.CurrentValue;

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
        _moveDirection = Vector3.ClampMagnitude(new Vector3(input.x, 0f, input.y), 1f);

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
