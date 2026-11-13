using UnityEngine;

// 플레이어의 기본 능력치. 강화는 이 값을 바꾸지 않고, 실행 중 값(PlayerStats)을 바꾼다.
[CreateAssetMenu(fileName = "PlayerStats", menuName = "REROOT/Player Stats Data")]
public class PlayerStatsData : ScriptableObject
{
    [Header("이동")]
    [SerializeField] private float _moveSpeed = 5f;
    [Tooltip("바라보는 방향으로 몸을 돌리는 속도(도/초)")]
    [SerializeField] private float _turnSpeed = 900f;

    [Header("공격")]
    [SerializeField] private int _damage = 1;
    [Tooltip("공격 버튼을 누르고 있는 동안 1초에 휘두르는 횟수")]
    [SerializeField] private float _attacksPerSecond = 1.8f;
    [Tooltip("플레이어 중심에서 타격 범위 중심까지의 거리. 나무에 붙어 섰을 때(중심 거리 약 0.65) 범위 중심이 밑동 근처에 오도록 둔다.")]
    [SerializeField] private float _attackReach = 0.75f;
    [Tooltip("타격 범위의 반지름. 나무 간격 1, 밑동 반지름 0.3 기준: 0.35면 정면 1그루·나무 사이 2그루, 0.65를 넘으면 3그루 이상.")]
    [SerializeField] private float _attackRadius = 0.35f;
    [Tooltip("바닥에서 타격 범위 중심까지의 높이. 나무 밑동 콜라이더에 닿는 높이로 둔다.")]
    [SerializeField] private float _attackHeight = 0.5f;

    public float MoveSpeed => _moveSpeed;
    public float TurnSpeed => _turnSpeed;
    public int Damage => _damage;
    public float AttacksPerSecond => _attacksPerSecond;
    public float AttackReach => _attackReach;
    public float AttackRadius => _attackRadius;
    public float AttackHeight => _attackHeight;
}
