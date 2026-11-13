using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

// 나무의 로직: 체력, 피해 판정, 쓰러짐 알림. 겉모습 연출은 TreeView에 맡긴다.
// 나무 종류를 늘릴 때는 이 클래스를 상속하고, 특별한 반응이 필요하면 OnDamaged/FallAsync를 재정의한다.
public abstract class Trees : MonoBehaviour, IDamageable
{
    [SerializeField] protected TreeData _data;
    [SerializeField] private Collider _collider;
    [SerializeField] private TreeView _view;

    [Header("상태")]
    [SerializeField, ReadOnly] private int _currentHp;

    private readonly Subject<Trees> _onHit = new();
    private readonly Subject<Trees> _onFelled = new();
    private CancellationTokenSource _activeCts;

    public TreeData Data => _data;
    public int CurrentHp => _currentHp;
    public bool IsFelled => _currentHp <= 0;
    public bool CanTakeDamage => !IsFelled;

    // 맞을 때마다 발행한다. 이번 타격으로 쓰러졌으면 IsFelled가 true다.
    public Observable<Trees> OnHit => _onHit;

    // 쓰러지는 연출까지 끝나서 풀에 돌아갈 준비가 됐을 때 발행한다.
    public Observable<Trees> OnFelled => _onFelled;

    protected virtual void OnEnable()
    {
        _activeCts = new CancellationTokenSource();
    }

    protected virtual void OnDisable()
    {
        // 풀에 돌아가면 진행 중인 쓰러짐을 멈춘다.
        _activeCts.Cancel();
        _activeCts.Dispose();
        _activeCts = null;
    }

    protected virtual void OnDestroy()
    {
        _onHit.Dispose();
        _onFelled.Dispose();
    }

    // 풀에서 꺼낼 때마다 호출된다. 처음 생성한 것처럼 되돌린다.
    public virtual void ResetState()
    {
        _currentHp = _data.MaxHp;
        _collider.enabled = true;
        _view.ResetView();
    }

    public void TakeDamage(int damage)
    {
        if (!CanTakeDamage)
        {
            return;
        }

        _currentHp = Mathf.Max(0, _currentHp - damage);
        _onHit.OnNext(this);

        if (IsFelled)
        {
            FallAndNotifyAsync(_activeCts.Token).Forget();
        }
        else
        {
            OnDamaged();
        }
    }

    // 맞았지만 아직 서 있을 때의 반응
    protected virtual void OnDamaged()
    {
        _view.PlayHit((float)_currentHp / _data.MaxHp);
    }

    // 쓰러지는 연출. 애니메이션이나 특수 효과가 필요한 나무는 재정의한다.
    protected virtual UniTask FallAsync(CancellationToken token)
    {
        return _view.PlayFallAsync(token);
    }

    private async UniTaskVoid FallAndNotifyAsync(CancellationToken token)
    {
        // 쓰러지는 동안 플레이어 길을 막거나 다시 맞지 않도록 콜라이더를 끈다.
        _collider.enabled = false;

        await FallAsync(token);
        _onFelled.OnNext(this);
    }
}
