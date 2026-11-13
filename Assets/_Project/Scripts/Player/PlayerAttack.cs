using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using VContainer;

// 공격 버튼을 누르고 있는 동안 바라보는 방향의 범위 안 대상(IDamageable)을 모두 때린다.
// 나무만 아는 게 아니라 IDamageable만 알기 때문에 바위 같은 새 대상도 그대로 맞는다.
public class PlayerAttack : MonoBehaviour
{
    private const int INITIAL_HIT_BUFFER_SIZE = 32;

    [Header("타이밍 (임시)")]
    [Tooltip("휘두르기 시작부터 실제로 맞을 때까지의 시간. 나중에 휘두르기 애니메이션의 타격 프레임에 맞춘다.")]
    [SerializeField] private float _hitDelay = 0.1f;

    private readonly List<IDamageable> _targets = new();
    private readonly Subject<Unit> _onSwing = new();
    private Collider[] _hitCandidates = new Collider[INITIAL_HIT_BUFFER_SIZE];
    private PlayerMovement _movement;
    private PlayerStats _stats;
    private InputAction _attackAction;
    private bool _isAttacking;
    private Vector3 _lastAttackPoint;

    // 휘두를 때마다 발행한다. 효과음, 애니메이션이 구독한다.
    public Observable<Unit> OnSwing => _onSwing;

    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
    }

    // PlayerInput이 OnEnable에서 액션을 준비한 뒤에 가져온다.
    private void Start()
    {
        _attackAction = GetComponent<PlayerInput>().actions["Attack"];
    }

    // 공격 버튼을 누르고 있는 동안 공격 속도에 맞춰 계속 휘두른다.
    // 입력이 꺼지면(일시정지, 세션 종료) 액션도 꺼져서 IsPressed가 false가 된다.
    private void Update()
    {
        if (_isAttacking || _stats == null || _attackAction == null || !_attackAction.IsPressed() || IsPointerOverUi())
        {
            return;
        }

        AttackAsync(destroyCancellationToken).Forget();
    }

    private void OnDestroy()
    {
        _onSwing.Dispose();
    }

    [Inject]
    public void Construct(PlayerStats stats)
    {
        _stats = stats;
    }

    // 일시정지 버튼 같은 UI 위에서 누른 것으로는 휘두르지 않는다.
    private static bool IsPointerOverUi()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private async UniTaskVoid AttackAsync(CancellationToken token)
    {
        _isAttacking = true;
        _onSwing.OnNext(Unit.Default);

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_hitDelay), cancellationToken: token);

            // 휘두르는 도중에 방향을 바꾸면 바뀐 방향으로 맞는다.
            Vector3 attackPoint = transform.position
                                  + _movement.FacingDirection * _stats.AttackReach.CurrentValue
                                  + Vector3.up * _stats.AttackHeight.CurrentValue;
            CollectTargets(attackPoint, _stats.AttackRadius.CurrentValue);

            // 맞은 대상이 쓰러지면서 풀로 돌아가도 목록이 흔들리지 않도록 미리 모아 둔 뒤 때린다.
            int damage = _stats.Damage.CurrentValue;
            foreach (IDamageable target in _targets)
            {
                target.TakeDamage(damage);
            }

            _targets.Clear();

            // 공격 속도만큼 다음 공격을 막는다.
            float cooldown = 1f / _stats.AttacksPerSecond.CurrentValue - _hitDelay;
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

    // 타격 범위에 걸친, 피해를 받을 수 있는 대상을 모두 모은다.
    private void CollectTargets(Vector3 attackPoint, float radius)
    {
        _lastAttackPoint = attackPoint;
        _targets.Clear();

        int count = OverlapAll(attackPoint, radius);
        for (int i = 0; i < count; i++)
        {
            if (_hitCandidates[i].TryGetComponent(out IDamageable target) && target.CanTakeDamage && !_targets.Contains(target))
            {
                _targets.Add(target);
            }
        }
    }

    // 버퍼가 꽉 차면 범위 안 대상을 놓쳤을 수 있으므로 버퍼를 키워 다시 찾는다.
    private int OverlapAll(Vector3 attackPoint, float radius)
    {
        while (true)
        {
            int count = Physics.OverlapSphereNonAlloc(
                attackPoint, radius, _hitCandidates, Physics.AllLayers, QueryTriggerInteraction.Ignore);

            if (count < _hitCandidates.Length)
            {
                return count;
            }

            _hitCandidates = new Collider[_hitCandidates.Length * 2];
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_stats == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_lastAttackPoint, _stats.AttackRadius.CurrentValue);
    }
}
