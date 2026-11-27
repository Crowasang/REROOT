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

    [Header("타이밍")]
    [Tooltip("한 번 휘두르는 시간 중 실제로 맞는 지점(0~1). 베기 애니메이션에서 도끼가 내려와 닿는 순간에 맞춘다. " +
             "시간이 아닌 비율이라 공격 속도를 강화해도 동작과 판정이 어긋나지 않는다.")]
    [SerializeField, Range(0f, 1f)] private float _hitTiming = 0.5f;

    private readonly List<IDamageable> _targets = new();
    private readonly Subject<Unit> _onSwing = new();
    private Collider[] _hitCandidates = new Collider[INITIAL_HIT_BUFFER_SIZE];
    private PlayerMovement _movement;
    private PlayerStats _stats;
    private InputAction _attackAction;
    private bool _isHolding;
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
        _attackAction.performed += OnAttackPerformed;
    }

    private void OnDestroy()
    {
        if (_attackAction != null)
        {
            _attackAction.performed -= OnAttackPerformed;
        }

        _onSwing.Dispose();
    }

    [Inject]
    public void Construct(PlayerStats _stats)
    {
        this._stats = _stats;
    }

    // 일시정지 버튼 같은 UI 위에서 누른 것으로는 휘두르지 않는다.
    private static bool IsPointerOverUi()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    // 공격 버튼을 누르는 순간(performed) 휘두르기를 시작한다.
    private void OnAttackPerformed(InputAction.CallbackContext _context)
    {
        if (!_isHolding)
        {
            HoldAttackAsync(destroyCancellationToken).Forget();
        }
    }

    // 버튼을 누르고 있는 동안 공격 속도에 맞춰 계속 휘두른다.
    // 입력이 꺼지면(일시정지, 세션 종료) 액션도 꺼져서 IsPressed가 false가 되고 반복이 끝난다.
    private async UniTaskVoid HoldAttackAsync(CancellationToken _token)
    {
        _isHolding = true;
        try
        {
            while (_attackAction.IsPressed())
            {
                if (_stats != null && !IsPointerOverUi())
                {
                    await AttackAsync(_token);
                }
                else
                {
                    await UniTask.Yield(_token);
                }
            }
        }
        finally
        {
            _isHolding = false;
        }
    }

    private async UniTask AttackAsync(CancellationToken _token)
    {
        _onSwing.OnNext(Unit.Default);

        // 한 번 휘두르는 시간을 공격 속도에서 구하고, 그 안에서 맞는 시점과 남은 시간을 나눈다.
        float _swingDuration = 1f / _stats.AttacksPerSecond.CurrentValue;
        float _hitDelay = _swingDuration * _hitTiming;

        await UniTask.Delay(TimeSpan.FromSeconds(_hitDelay), cancellationToken: _token);

        // 휘두르는 도중에 방향을 바꾸면 바뀐 방향으로 맞는다.
        Vector3 _attackPoint = transform.position
                              + _movement.FacingDirection * _stats.AttackReach.CurrentValue
                              + Vector3.up * _stats.AttackHeight.CurrentValue;
        CollectTargets(_attackPoint, _stats.AttackRadius.CurrentValue);

        // 맞은 대상이 쓰러지면서 풀로 돌아가도 목록이 흔들리지 않도록 미리 모아 둔 뒤 때린다.
        int _damage = _stats.Damage.CurrentValue;
        foreach (IDamageable _target in _targets)
        {
            _target.TakeDamage(_damage);
        }

        _targets.Clear();

        // 공격 속도만큼 다음 공격을 막는다.
        float _cooldown = _swingDuration - _hitDelay;
        if (_cooldown > 0f)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_cooldown), cancellationToken: _token);
        }
    }

    // 타격 범위에 걸친, 피해를 받을 수 있는 대상을 모두 모은다.
    private void CollectTargets(Vector3 _attackPoint, float _radius)
    {
        _lastAttackPoint = _attackPoint;
        _targets.Clear();

        int _count = OverlapAll(_attackPoint, _radius);
        for (int _i = 0; _i < _count; _i++)
        {
            if (_hitCandidates[_i].TryGetComponent(out IDamageable _target) && _target.CanTakeDamage && !_targets.Contains(_target))
            {
                _targets.Add(_target);
            }
        }
    }

    // 버퍼가 꽉 차면 범위 안 대상을 놓쳤을 수 있으므로 버퍼를 키워 다시 찾는다.
    private int OverlapAll(Vector3 _attackPoint, float _radius)
    {
        while (true)
        {
            int _count = Physics.OverlapSphereNonAlloc(
                _attackPoint, _radius, _hitCandidates, Physics.AllLayers, QueryTriggerInteraction.Ignore);

            if (_count < _hitCandidates.Length)
            {
                return _count;
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
