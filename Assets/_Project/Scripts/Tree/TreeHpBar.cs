using UnityEngine;

// 나무 위에 뜨는 체력 바. 첫 타격 전에는 숨겨 둔다.
public class TreeHpBar : MonoBehaviour
{
    [Tooltip("가운데 피벗 스프라이트. 체력 비율만큼 가로로 줄이고 왼쪽 끝에 맞춘다.")]
    [SerializeField] private Transform _fill;

    private Vector3 _fillBaseScale;
    private Vector3 _fillBasePosition;
    private bool _isInitialized;
    private Transform _cameraTransform;

    // 3D 공간에 떠 있으므로 늘 카메라를 정면으로 보게 돌린다.
    private void LateUpdate()
    {
        if (_cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            _cameraTransform = mainCamera.transform;
        }

        transform.rotation = _cameraTransform.rotation;
    }

    public void Show(float ratio)
    {
        Initialize();
        gameObject.SetActive(true);

        ratio = Mathf.Clamp01(ratio);
        _fill.localScale = new Vector3(_fillBaseScale.x * ratio, _fillBaseScale.y, _fillBaseScale.z);
        _fill.localPosition = _fillBasePosition - new Vector3(_fillBaseScale.x * (1f - ratio) * 0.5f, 0f, 0f);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // 꺼진 상태로 시작하므로 Awake 대신 처음 보여줄 때 기준값을 기억한다.
    private void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        _fillBaseScale = _fill.localScale;
        _fillBasePosition = _fill.localPosition;
        _isInitialized = true;
    }
}
