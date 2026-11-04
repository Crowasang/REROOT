using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    private const int MAX_HIT_CANDIDATES = 16;

    [SerializeField] private int _damage = 1;
    [SerializeField] private float _attacksPerSecond = 1.8f;

    [Header("타격 범위")]
    [Tooltip("플레이어 중심에서 타격 지점까지의 거리. 바라보는 방향으로 뻗는다.")]
    [SerializeField] private float _attackReach = 0.8f;
    [SerializeField] private float _attackRadius = 0.5f;

    [Header("타이밍 (임시)")]
    [Tooltip("클릭 후 실제로 맞을 때까지의 시간. 나중에 휘두르기 애니메이션의 타격 프레임에 맞춘다.")]
    [SerializeField] private float _hitDelay = 0.1f;

    private readonly Collider2D[] _hitCandidates = new Collider2D[MAX_HIT_CANDIDATES];
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
            Trees target = FindTarget((Vector2)transform.position + _movement.FacingDirection * _attackReach);
            if (target != null)
            {
                target.TakeDamage(_damage);
            }

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

    // 타격 지점에 가장 가까운 나무 한 그루를 고른다.
    private Trees FindTarget(Vector2 attackPoint)
    {
        _lastAttackPoint = attackPoint;

        int count = Physics2D.OverlapCircle(attackPoint, _attackRadius, _contactFilter, _hitCandidates);

        Trees closest = null;
        float closestSqr = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            if (!_hitCandidates[i].TryGetComponent(out Trees tree) || tree.IsFelled)
            {
                continue;
            }

            float sqr = ((Vector2)tree.transform.position - attackPoint).sqrMagnitude;
            if (sqr < closestSqr)
            {
                closestSqr = sqr;
                closest = tree;
            }
        }

        return closest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_lastAttackPoint, _attackRadius);
    }
}
