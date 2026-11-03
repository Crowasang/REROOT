using Unity.Cinemachine;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private CinemachineConfiner2D _confiner;
    [SerializeField] private PolygonCollider2D _bounds;

    private readonly Vector2[] _boundsPath = new Vector2[4];

    public void SetTarget(Transform target)
    {
        _camera.Follow = target;

        // 시작할 때 이전 위치에서 미끄러져 오지 않고 바로 대상 위치에서 시작한다.
        _camera.PreviousStateIsValid = false;
    }

    public void SetBounds(Rect rect)
    {
        _boundsPath[0] = new Vector2(rect.xMin, rect.yMin);
        _boundsPath[1] = new Vector2(rect.xMin, rect.yMax);
        _boundsPath[2] = new Vector2(rect.xMax, rect.yMax);
        _boundsPath[3] = new Vector2(rect.xMax, rect.yMin);

        _bounds.pathCount = 1;
        _bounds.SetPath(0, _boundsPath);
        _confiner.InvalidateBoundingShapeCache();
    }
}
