using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 나무의 겉모습 연출만 맡는다. 체력·판정 같은 로직은 Trees가 맡는다.
// 모델을 VARCO 모델로 바꾸거나 애니메이션을 넣을 때는 이 클래스만 고치면 된다.
public class TreeView : MonoBehaviour
{
    // URP Lit/Unlit 셰이더의 기본 색 속성
    private static readonly int BASE_COLOR_ID = Shader.PropertyToID("_BaseColor");

    [Tooltip("모델이 들어 있는 자식. 흔들림 연출이 이 위치를 움직인다. 모델을 바꿀 때는 이 아래만 교체한다.")]
    [SerializeField] private Transform _visual;
    [SerializeField] private TreeHpBar _hpBar;

    [Header("피격 연출 (임시)")]
    [SerializeField] private float _hitShakeDuration = 0.15f;
    [SerializeField] private float _hitShakeStrength = 0.12f;
    [SerializeField] private Color _hitFlashColor = new(1f, 0.3f, 0.3f);

    [Header("쓰러짐 (임시)")]
    [Tooltip("쓰러지는 연출 길이. 이 시간이 지나야 나무가 풀로 돌아간다. 애니메이션을 넣으면 그 길이로 맞춘다.")]
    [SerializeField] private float _fallDuration = 0.3f;
    [SerializeField] private Color _fallingColor = new(0.5f, 0.5f, 0.5f);

    private Renderer[] _renderers;
    private MaterialPropertyBlock _tintBlock;
    private Vector3 _visualBasePosition;
    private CancellationTokenSource _hitCts;
    private bool _isInitialized;

    private void OnDisable()
    {
        CancelHit();
    }

    // 풀에서 꺼낼 때마다 처음 모습으로 되돌린다.
    public void ResetView()
    {
        Initialize();
        CancelHit();
        _visual.localPosition = _visualBasePosition;
        ClearTint();
        _hpBar.Hide();
    }

    public void PlayHit(float hpRatio)
    {
        Initialize();
        _hpBar.Show(hpRatio);

        // 연달아 맞으면 앞 흔들림을 끊고 새로 흔든다.
        CancelHit();
        _hitCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        PlayHitAsync(_hitCts.Token).Forget();
    }

    public async UniTask PlayFallAsync(CancellationToken token)
    {
        Initialize();
        CancelHit();
        _hpBar.Hide();
        _visual.localPosition = _visualBasePosition;
        SetTint(_fallingColor);

        await UniTask.Delay(TimeSpan.FromSeconds(_fallDuration), cancellationToken: token);
    }

    // 프리팹이 꺼진 채로 만들어질 수 있으므로 Awake 대신 처음 쓸 때 준비한다.
    private void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        // 모델은 여러 메시로 이뤄질 수 있으므로 자식 렌더러를 모두 물들인다.
        _renderers = _visual.GetComponentsInChildren<Renderer>();
        _tintBlock = new MaterialPropertyBlock();
        _visualBasePosition = _visual.localPosition;
        _isInitialized = true;
    }

    private async UniTaskVoid PlayHitAsync(CancellationToken token)
    {
        SetTint(_hitFlashColor);

        try
        {
            float elapsed = 0f;
            while (elapsed < _hitShakeDuration)
            {
                // 바닥과 나란하게(XZ) 흔든다.
                Vector2 offset = UnityEngine.Random.insideUnitCircle * _hitShakeStrength;
                _visual.localPosition = _visualBasePosition + new Vector3(offset.x, 0f, offset.y);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
                elapsed += Time.deltaTime;
            }

            _visual.localPosition = _visualBasePosition;
            ClearTint();
        }
        catch (OperationCanceledException)
        {
            // 쓰러짐이나 다음 타격이 연출을 이어받는다.
        }
    }

    private void CancelHit()
    {
        if (_hitCts == null)
        {
            return;
        }

        _hitCts.Cancel();
        _hitCts.Dispose();
        _hitCts = null;
    }

    // 머티리얼을 복제하지 않고 색만 덮어쓴다. 나무 수백 그루가 머티리얼을 공유하기 때문이다.
    private void SetTint(Color color)
    {
        _tintBlock.SetColor(BASE_COLOR_ID, color);
        foreach (Renderer target in _renderers)
        {
            target.SetPropertyBlock(_tintBlock);
        }
    }

    // 덮어쓴 색을 지워 머티리얼 원래 색으로 돌린다.
    private void ClearTint()
    {
        foreach (Renderer target in _renderers)
        {
            target.SetPropertyBlock(null);
        }
    }
}
