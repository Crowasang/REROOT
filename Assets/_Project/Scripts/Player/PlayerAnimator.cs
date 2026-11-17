using R3;
using UnityEngine;
using VContainer;

// 플레이어 애니메이션만 맡는다: 움직이는 빠르기로 대기·달리기를 섞고, 휘두를 때 베기를 튼다.
// 이동·공격 로직은 모르고, 그 결과(속도, 휘두르기 이벤트)만 받아서 애니메이터에 넘긴다.
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int SPEED_ID = Animator.StringToHash("Speed");
    private static readonly int CHOP_ID = Animator.StringToHash("Chop");
    private static readonly int ATTACK_SPEED_ID = Animator.StringToHash("AttackSpeed");

    [Tooltip("모델에 붙은 애니메이터(휴머노이드)")]
    [SerializeField] private Animator _animator;
    [Tooltip("베기 상태에 쓴 클립. 길이를 읽어 공격 속도에 맞게 재생 속도를 정한다.")]
    [SerializeField] private AnimationClip _chopClip;
    [Tooltip("달리기 ↔ 대기 전환을 부드럽게 하는 시간(초)")]
    [SerializeField] private float _speedDampTime = 0.08f;

    private PlayerMovement _movement;
    private PlayerAttack _attack;
    private PlayerStats _stats;

    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        _attack = GetComponent<PlayerAttack>();
        _animator.applyRootMotion = false;
    }

    private void Start()
    {
        _attack.OnSwing.Subscribe(_ => PlayChop()).AddTo(this);
    }

    private void Update()
    {
        _animator.SetFloat(SPEED_ID, _movement.NormalizedSpeed, _speedDampTime, Time.deltaTime);
    }

    [Inject]
    public void Construct(PlayerStats stats)
    {
        _stats = stats;
    }

    private void PlayChop()
    {
        // 클립 한 번이 휘두르기 한 번(1 / 공격 속도) 안에 끝나도록 재생 속도를 맞춘다.
        if (_stats != null && _chopClip != null)
        {
            _animator.SetFloat(ATTACK_SPEED_ID, _chopClip.length * _stats.AttacksPerSecond.CurrentValue);
        }

        _animator.SetTrigger(CHOP_ID);
    }
}
