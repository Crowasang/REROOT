using Unity.Cinemachine;
using UnityEngine;

// 기울어진 카메라가 플레이어를 따라간다.
// 카메라를 직접 가두지 않고, 카메라가 따라가는 지점(포커스)을 바닥 범위 안에 가둔다.
public class GameCamera : MonoBehaviour, ICameraController
{
    [SerializeField] private CinemachineCamera _camera;
    [Tooltip("카메라가 실제로 따라가는 지점. 플레이어 위치를 범위 안으로 잘라서 옮긴다.")]
    [SerializeField] private Transform _focus;
    [Tooltip("범위 가장자리에서 포커스를 안쪽으로 들여 둘 거리(x: 좌우, y: 앞뒤). " +
             "화면 절반 정도로 두면 맵 바깥 빈 곳이 덜 보인다.")]
    [SerializeField] private Vector2 _edgeInset = new(9f, 4f);

    private Transform _target;
    private Rect _bounds;
    private bool _hasBounds;

    // Cinemachine은 LateUpdate에서 카메라를 옮기므로, 그보다 먼저 Update에서 포커스를 옮긴다.
    private void Update()
    {
        if (_target != null)
        {
            _focus.position = ClampToBounds(_target.position);
        }
    }

    public void SetTarget(Transform target)
    {
        _target = target;
        _focus.position = ClampToBounds(target.position);
        _camera.Follow = _focus;

        // 시작할 때 이전 위치에서 미끄러져 오지 않고 바로 대상 위치에서 시작한다.
        _camera.PreviousStateIsValid = false;
    }

    // rect의 x는 월드 X, y는 월드 Z 범위다.
    public void SetBounds(Rect rect)
    {
        _bounds = rect;
        _hasBounds = true;
    }

    private Vector3 ClampToBounds(Vector3 position)
    {
        if (!_hasBounds)
        {
            return new Vector3(position.x, 0f, position.z);
        }

        return new Vector3(
            ClampAxis(position.x, _bounds.xMin + _edgeInset.x, _bounds.xMax - _edgeInset.x),
            0f,
            ClampAxis(position.z, _bounds.yMin + _edgeInset.y, _bounds.yMax - _edgeInset.y));
    }

    // 범위가 들여쓰기보다 좁으면 가운데에 고정한다.
    private static float ClampAxis(float value, float min, float max)
    {
        return min <= max ? Mathf.Clamp(value, min, max) : (min + max) * 0.5f;
    }
}
