using R3;
using UnityEngine;

public abstract class Trees : MonoBehaviour
{
    [SerializeField] protected TreeData _data;

    protected int _currentHp;

    private readonly Subject<Trees> _onFelled = new();

    public TreeData Data => _data;
    public bool IsFelled => _currentHp <= 0;

    // 나무가 다 쓰러져서 풀에 돌아갈 준비가 됐을 때 발행한다.
    public Observable<Trees> OnFelled => _onFelled;

    protected virtual void Awake()
    {
        ResetState();
    }

    protected virtual void OnDestroy()
    {
        _onFelled.Dispose();
    }

    // 풀에서 꺼낼 때마다 호출된다. 상태를 처음 생성한 것처럼 되돌린다.
    public virtual void ResetState()
    {
        _currentHp = _data.MaxHp;
    }

    protected void TakeDamage(int damage)
    {
        if (IsFelled)
        {
            return;
        }

        _currentHp -= damage;
        if (_currentHp <= 0)
        {
            _currentHp = 0;
            Fall();
        }
    }

    // 쓰러짐 연출을 넣을 때는 재정의해서 연출이 끝난 뒤 base.Fall()을 호출한다.
    protected virtual void Fall()
    {
        _onFelled.OnNext(this);
    }
}
