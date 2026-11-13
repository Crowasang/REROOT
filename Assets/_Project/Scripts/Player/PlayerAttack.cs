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
    [Tooltip("공격 버튼을 누르고 있는 동안 1초에 휘두르는 횟수. 공격 속도 강화로 올린다.")]
    [SerializeField] private float _attacksPerSecond = 1.8f;

    [Header("타격 범위")]
    [Tooltip("플레이어 중심에서 타격 범위 중심까지의 거리. 바라보는 방향으로 뻗는다. " +
             "나무에 붙어 섰을 때(중심 거리 약 0.65) 범위 중심이 밑동 근처에 오도록 둔다.")]
    [SerializeField] private float _attackReach = 0.75f;
    [Tooltip("타격 범위의 반지름. 범위에 걸친 나무는 모두 맞는다. 강화로 넓힌다. " +
             "나무 간격 1, 밑동 반지름 0.3 기준: 0.35면 정면 1그루·나무 사이 2그루, 0.65를 넘으면 3그루 이상.")]
    [SerializeField] private float _attackRadius = 0.35f;
    [Tooltip("바닥에서 타격 범위 중심까지의 높이. 나무 밑동 콜라이더에 닿는 높이로 둔다.")]
    [SerializeField] private float _attackHeight = 0.5f;

    [Header("타이밍 (임시)")]
    [Tooltip("휘두르기 시작부터 실제로 맞을 때까지의 시간. 나중에 휘두르기 애니메이션의 타격 프레임에 맞춘다.")]
    [SerializeField] private float _hitDelay = 0.1f;

    private readonly List<Trees> _targets = new();
    private Collider[] _hitCandidates = new Collider[INITIAL_HIT_BUFFER_SIZE];
    private PlayerMovement _movement;
    private InputAction _attackAction;
    private bool _isAttacking;
    private Vector3 _lastAttackPoint;

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
        if (_isAttacking || _attackAction == null || !_attackAction.IsPressed() || IsPointerOverUi())
        {
            return;
        }

        AttackAsync(destroyCancellationToken).Forget();
    }

    // 일시정지 버튼 같은 UI 위에서 누른 것으로는 휘두르지 않는다.
    private static bool IsPointerOverUi()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
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
