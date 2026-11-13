using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    private const int INITIAL_HIT_BUFFER_SIZE = 32;

    [SerializeField] private int _damage = 1;
    [SerializeField] private float _attacksPerSecond = 1.8f;

    [Header("타격 범위")]
    [Tooltip("플레이어 중심에서 타격 범위 중심까지의 거리. 바라보는 방향으로 뻗는다.")]
    [SerializeField] private float _attackReach = 0.8f;
    [Tooltip("타격 범위의 반지름. 범위에 걸친 나무는 모두 맞는다.")]
    [SerializeField] private float _attackRadius = 0.5f;
    [Tooltip("바닥에서 타격 범위 중심까지의 높이. 나무 밑동 콜라이더에 닿는 높이로 둔다.")]
    [SerializeField] private float _attackHeight = 0.5f;

    [Header("타이밍 (임시)")]
    [Tooltip("클릭 후 실제로 맞을 때까지의 시간. 나중에 휘두르기 애니메이션의 타격 프레임에 맞춘다.")]
    [SerializeField] private float _hitDelay = 0.1f;

    private readonly List<Trees> _targets = new();
    private Collider[] _hitCandidates = new Collider[INITIAL_HIT_BUFFER_SIZE];
    private PlayerMovement _movement;
    private bool _isAttacking;
    private bool _isPointerOverUi;
    private Vector3 _lastAttackPoint;

    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
    }

    // 입력 콜백 안에서는 UI 판정이 한 프레임 늦으므로 Update에서 미리 읽어 둔다.
    private void Update()
    {
        _isPointerOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    // PlayerInput(Send Messages)이 Attack 액션에서 호출한다.
    public void OnAttack(InputValue value)
    {
        // 일시정지 버튼 같은 UI를 누른 클릭으로는 휘두르지 않는다.
        if (value.isPressed && !_isAttacking && !_isPointerOverUi)
        {
            AttackAsync(destroyCancellationToken).Forget();
        }
    }

    private async UniTaskVoid AttackAsync(CancellationToken token)
    {
        _isAttacking = true;

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_hitDelay), cancellationToken: token);

            // 휘두르는 도중에 방향을 바꾸면 바뀐 방향으로 맞는다.
            Vector3 attackPoint = transform.position
                                  + _movement.FacingDirection * _attackReach
                                  + Vector3.up * _attackHeight;
            CollectTargets(attackPoint);

            // 맞은 나무가 쓰러지면서 풀로 돌아가도 목록이 흔들리지 않도록 미리 모아 둔 뒤 때린다.
            foreach (Trees tree in _targets)
            {
                tree.TakeDamage(_damage);
            }

            _targets.Clear();

            // 공격 속도만큼 다음 공격을 막는다.
            float cooldown = 1f / _attacksPerSecond - _hitDelay;
            if (cooldown > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(cooldown), cancellationToken: token);
            }
        }
        finally
        {
            _isAttacking = false;
        }
    }

    // 타격 범위에 걸친, 아직 서 있는 나무를 모두 모은다.
    private void CollectTargets(Vector3 attackPoint)
    {
        _lastAttackPoint = attackPoint;
        _targets.Clear();

        int count = OverlapAll(attackPoint);
        for (int i = 0; i < count; i++)
        {
            if (_hitCandidates[i].TryGetComponent(out Trees tree) && !tree.IsFelled && !_targets.Contains(tree))
            {
                _targets.Add(tree);
            }
        }
    }

    // 버퍼가 꽉 차면 범위 안 나무를 놓쳤을 수 있으므로 버퍼를 키워 다시 찾는다.
    private int OverlapAll(Vector3 attackPoint)
    {
        while (true)
        {
            int count = Physics.OverlapSphereNonAlloc(
                attackPoint, _attackRadius, _hitCandidates, Physics.AllLayers, QueryTriggerInteraction.Ignore);

            if (count < _hitCandidates.Length)
            {
                return count;
            }

            _hitCandidates = new Collider[_hitCandidates.Length * 2];
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_lastAttackPoint, _attackRadius);
    }
}
