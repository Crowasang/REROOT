using UnityEngine;

// 벌목 영역 바깥을 피해를 받지 않는 나무로 빈틈없이 둘러싼다.
public class MapBoundary : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _spawnArea;
    [SerializeField] private GameObject _boundaryTreePrefab;
    [SerializeField] private Transform _boundaryRoot;

    [Tooltip("벌목 영역과 경계 나무 줄 사이에 남길 거리. 플레이어가 가장자리 나무를 돌아서 지나갈 수 있게 한다.")]
    [SerializeField] private float _padding = 1.5f;
    [Tooltip("경계 나무 사이 간격. 나무 지름보다 작아야 빈틈이 생기지 않는다.")]
    [SerializeField] private float _treeSpacing = 0.8f;
    [Tooltip("카메라가 경계 나무 바깥을 얼마나 더 보여줄지")]
    [SerializeField] private float _cameraMargin = 1f;

    private Transform BoundaryRoot => _boundaryRoot != null ? _boundaryRoot : transform;

    // 카메라가 넘어가면 안 되는 범위
    public Rect CameraRect { get; private set; }

    public void Build()
    {
        Clear();

        Bounds bounds = _spawnArea.bounds;
        bounds.Expand(new Vector3(_padding * 2f, _padding * 2f, 0f));
        Vector2 min = bounds.min;
        Vector2 max = bounds.max;

        int columns = Mathf.CeilToInt((max.x - min.x) / _treeSpacing);
        int rows = Mathf.CeilToInt((max.y - min.y) / _treeSpacing);
        float stepX = (max.x - min.x) / columns;
        float stepY = (max.y - min.y) / rows;

        // 위아래 줄은 모서리까지, 양옆 줄은 모서리를 빼고 놓아 겹치지 않게 한다.
        for (int x = 0; x <= columns; x++)
        {
            PlaceTree(new Vector2(min.x + x * stepX, min.y));
            PlaceTree(new Vector2(min.x + x * stepX, max.y));
        }

        for (int y = 1; y < rows; y++)
        {
            PlaceTree(new Vector2(min.x, min.y + y * stepY));
            PlaceTree(new Vector2(max.x, min.y + y * stepY));
        }

        Vector2 margin = Vector2.one * _cameraMargin;
        CameraRect = Rect.MinMaxRect(min.x - margin.x, min.y - margin.y, max.x + margin.x, max.y + margin.y);
    }

    private void Clear()
    {
        for (int i = BoundaryRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(BoundaryRoot.GetChild(i).gameObject);
        }
    }

    private void PlaceTree(Vector2 position)
    {
        Instantiate(_boundaryTreePrefab, position, Quaternion.identity, BoundaryRoot);
    }
}
