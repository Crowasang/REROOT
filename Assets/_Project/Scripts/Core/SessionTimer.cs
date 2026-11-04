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
    // 시간이 다 돼서 끝났을 때만 발행한다.
    public Observable<Unit> OnEnded => _onEnded;

    private void Update()
    {
        if (!_isRunning)
        {
            return;
        }

        // Time.timeScale이 0이면 deltaTime도 0이라 일시정지 중에는 시간이 줄지 않는다.
        _remaining.Value = Mathf.Max(0f, _remaining.Value - Time.deltaTime);
        if (_remaining.Value <= 0f)
        {
            _isRunning = false;
            _onEnded.OnNext(Unit.Default);
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

    // 시간 말고 다른 이유(숲을 다 베거나 직접 나감)로 끝날 때 멈춘다. OnEnded는 발행하지 않는다.
    public void Stop()
    {
        _isRunning = false;
    }
}
