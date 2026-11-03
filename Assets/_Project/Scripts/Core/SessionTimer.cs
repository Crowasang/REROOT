using R3;
using UnityEngine;

// 숲에 머물 수 있는 시간. 화면에는 플레이어 체력 바처럼 보여준다.
public class SessionTimer : MonoBehaviour
{
    [SerializeField] private float _baseDuration = 30f;
    [Tooltip("나무 한 그루를 벨 때마다 늘어나는 시간")]
    [SerializeField] private float _bonusPerTree = 1f;

    [Header("상태")]
    [SerializeField, ReadOnly] private bool _isRunning;

    private readonly ReactiveProperty<float> _remaining = new(0f);
    private readonly Subject<Unit> _onEnded = new();

    public float BaseDuration => _baseDuration;
    public bool IsRunning => _isRunning;
    public ReadOnlyReactiveProperty<float> Remaining => _remaining;
    public Observable<Unit> OnEnded => _onEnded;

    private void Update()
    {
        if (!_isRunning)
        {
            return;
        }

        _remaining.Value = Mathf.Max(0f, _remaining.Value - Time.deltaTime);
        if (_remaining.Value <= 0f)
        {
            End();
        }
    }

    private void OnDestroy()
    {
        _remaining.Dispose();
        _onEnded.Dispose();
    }

    public void Begin()
    {
        _remaining.Value = _baseDuration;
        _isRunning = true;
    }

    public void AddTreeBonus()
    {
        if (_isRunning)
        {
            _remaining.Value += _bonusPerTree;
        }
    }

    // 시간이 다 되거나, 플레이어가 직접 나갈 때 호출한다.
    public void End()
    {
        if (!_isRunning)
        {
            return;
        }

        _isRunning = false;
        _onEnded.OnNext(Unit.Default);
    }
}
