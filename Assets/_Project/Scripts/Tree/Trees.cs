using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public abstract class Trees : MonoBehaviour
{
    [SerializeField] protected TreeData _data;
    [SerializeField] private Transform _visual;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private TreeHpBar _hpBar;

    [Header("피격 연출 (임시)")]
    [SerializeField] private float _hitShakeDuration = 0.12f;
    [SerializeField] private float _hitShakeStrength = 0.08f;
    [SerializeField] private Color _hitFlashColor = new(1f, 0.55f, 0.55f);

    [Header("쓰러짐 (임시)")]
    [Tooltip("체력이 0이 된 뒤 풀로 돌아가기까지 기다리는 시간. 쓰러짐 애니메이션을 넣으면 그 길이로 맞춘다.")]
    [SerializeField] private float _fallDuration = 0.3f;
    [SerializeField] private Color _fallingColor = new(0.5f, 0.5f, 0.5f);

    [Header("상태")]
    [SerializeField, ReadOnly] private int _currentHp;

    private readonly Subject<Trees> _onFelled = new();

    private SpriteRenderer _spriteRenderer;
    private Color _baseColor;
    private Vector3 _visualBasePosition;
    private CancellationTokenSource _activeCts;

    public TreeData Data => _data;
    public int CurrentHp => _currentHp;
    public bool IsFelled => _currentHp <= 0;

    // 나무가 다 쓰러져서 풀에 돌아갈 준비가 됐을 때 발행한다.
    public Observable<Trees> OnFelled => _onFelled;

    protected virtual void Awake()
    {
        _spriteRenderer = _visual.GetComponent<SpriteRenderer>();
        _baseColor = _spriteRenderer.color;
        _visualBasePosition = _visual.localPosition;
        ResetState();
    }

    protected virtual void OnEnable()
    {
        _activeCts = new CancellationTokenSource();
    }

    protected virtual void OnDisable()
    {
        // 풀에 돌아가면 진행 중인 연출을 모두 멈춘다.
        _activeCts.Cancel();
        _activeCts.Dispose();
        _activeCts = null;
    }

    protected virtual void OnDestroy()
    {
        _onFelled.Dispose();
    }

    // 풀에서 꺼낼 때마다 호출된다. 상태를 처음 생성한 것처럼 되돌린다.
    public virtual void ResetState()
    {
        _currentHp = _data.MaxHp;
        _collider.enabled = true;
        _visual.localPosition = _visualBasePosition;
        _spriteRenderer.color = _baseColor;
        _hpBar.Hide();
    }

    public void TakeDamage(int damage)
    {
        if (IsFelled)
        {
            return;
        }

        _currentHp = Mathf.Max(0, _currentHp - damage);

        if (IsFelled)
        {
            _hpBar.Hide();
            FallAndNotifyAsync(_activeCts.Token).Forget();
        }
        else
        {
            _hpBar.Show((float)_currentHp / _data.MaxHp);
            PlayHitAsync(_activeCts.Token).Forget();
        }
    }

    // 쓰러짐 연출. 애니메이션을 넣을 때는 재정의해서 애니메이션이 끝날 때까지 기다린다.
    protected virtual async UniTask FallAsync(CancellationToken token)
    {
        _spriteRenderer.color = _fallingColor;
        await UniTask.Delay(TimeSpan.FromSeconds(_fallDuration), cancellationToken: token);
    }

    private async UniTaskVoid FallAndNotifyAsync(CancellationToken token)
    {
        // 쓰러지는 동안 플레이어 길을 막거나 다시 맞지 않도록 콜라이더를 끈다.
        _collider.enabled = false;
        _visual.localPosition = _visualBasePosition;

        await FallAsync(token);
        _onFelled.OnNext(this);
    }

    private async UniTaskVoid PlayHitAsync(CancellationToken token)
    {
        _spriteRenderer.color = _hitFlashColor;

        try
        {
            float elapsed = 0f;
            while (elapsed < _hitShakeDuration && !IsFelled)
            {
                _visual.localPosition = _visualBasePosition + (Vector3)(UnityEngine.Random.insideUnitCircle * _hitShakeStrength);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
                elapsed += Time.deltaTime;
            }
        }
        finally
        {
            if (!IsFelled)
            {
                _visual.localPosition = _visualBasePosition;
                _spriteRenderer.color = _baseColor;
            }
        }
    }
}
