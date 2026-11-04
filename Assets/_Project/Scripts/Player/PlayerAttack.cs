using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _attacksPerSecond = 1.8f;

    [Header("타격 범위")]
    [Tooltip("플레이어 중심에서 타격 범위 중심까지의 거리. 바라보는 방향으로 뻗는다.")]
    [SerializeField] private float _attackReach = 0.8f;
    [Tooltip("타격 범위의 반지름. 범위에 걸친 나무는 모두 맞는다.")]
    [SerializeField] private float _attackRadius = 0.5f;

    [Header("타이밍 (임시)")]
    [Tooltip("클릭 후 실제로 맞을 때까지의 시간. 나중에 휘두르기 애니메이션의 타격 프레임에 맞춘다.")]
    [SerializeField] private float _hitDelay = 0.1f;

    // 범위가 커져도 잘리지 않도록 크기가 늘어나는 리스트를 쓴다.
    private readonly List<Collider2D> _hitCandidates = new();
    private readonly List<Trees> _targets = new();
    private ContactFilter2D _contactFilter;
    private PlayerMovement _movement;
    private bool _isAttacking;
    private Vector2 _lastAttackPoint;

    private void Awake()
    {
        _contactFilter = ContactFilter2D.noFilter;
        _movement = GetComponent<PlayerMovement>();
    }

    // PlayerInput(Send Messages)이 Attack 액션에서 호출한다.
    public void OnAttack(InputValue value)
    {
        if (value.isPressed && !_isAttacking)
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
            CollectTargets((Vector2)transform.position + _movement.FacingDirection * _attackReach);

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
    private void CollectTargets(Vector2 attackPoint)
    {
        _lastAttackPoint = attackPoint;
        _targets.Clear();

        Physics2D.OverlapCircle(attackPoint, _attackRadius, _contactFilter, _hitCandidates);

        foreach (Collider2D candidate in _hitCandidates)
        {
            if (candidate.TryGetComponent(out Trees tree) && !tree.IsFelled && !_targets.Contains(tree))
            {
                _targets.Add(tree);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_lastAttackPoint, _attackRadius);
    }
}
