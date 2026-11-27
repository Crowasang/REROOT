using UnityEngine;

// 벌목 영역 바깥을 피해를 받지 않는 나무로 빈틈없이 둘러싼다.
public class MapBoundary : MonoBehaviour, IMapBoundary
{
    [Tooltip("나무를 까는 영역. 바닥(XZ) 크기만 쓴다.")]
    [SerializeField] private BoxCollider _spawnArea;
    [SerializeField] private GameObject _boundaryTreePrefab;
    [SerializeField] private Transform _boundaryRoot;

    [Tooltip("벌목 영역과 경계 나무 줄 사이에 남길 거리. 플레이어가 가장자리 나무를 돌아서 지나갈 수 있게 한다.")]
    [SerializeField] private float _padding = 1.5f;
    [Tooltip("경계 나무 사이 간격. 나무 밑동 지름보다 작아야 빈틈이 생기지 않는다.")]
    [SerializeField] private float _treeSpacing = 0.8f;
    [Tooltip("카메라가 경계 나무 바깥을 얼마나 더 보여줄지")]
    [SerializeField] private float _cameraMargin = 1f;

    private Transform BoundaryRoot => _boundaryRoot != null ? _boundaryRoot : transform;

    // 카메라가 따라갈 수 있는 바닥 범위. x는 월드 X, y는 월드 Z다.
    public Rect CameraRect { get; private set; }

    public void Build()
    {
        Clear();

        Bounds _bounds = _spawnArea.bounds;
        float _minX = _bounds.min.x - _padding;
        float _maxX = _bounds.max.x + _padding;
        float _minZ = _bounds.min.z - _padding;
        float _maxZ = _bounds.max.z + _padding;

        int _columns = Mathf.CeilToInt((_maxX - _minX) / _treeSpacing);
        int _rows = Mathf.CeilToInt((_maxZ - _minZ) / _treeSpacing);
        float _stepX = (_maxX - _minX) / _columns;
        float _stepZ = (_maxZ - _minZ) / _rows;

        // 앞뒤 줄은 모서리까지, 양옆 줄은 모서리를 빼고 놓아 겹치지 않게 한다.
        for (int _x = 0; _x <= _columns; _x++)
        {
            PlaceTree(_minX + _x * _stepX, _minZ);
            PlaceTree(_minX + _x * _stepX, _maxZ);
        }

        for (int _z = 1; _z < _rows; _z++)
        {
            PlaceTree(_minX, _minZ + _z * _stepZ);
            PlaceTree(_maxX, _minZ + _z * _stepZ);
        }

        CameraRect = Rect.MinMaxRect(_minX - _cameraMargin, _minZ - _cameraMargin, _maxX + _cameraMargin, _maxZ + _cameraMargin);
    }

    private void Clear()
    {
        for (int _i = BoundaryRoot.childCount - 1; _i >= 0; _i--)
        {
            Destroy(BoundaryRoot.GetChild(_i).gameObject);
        }
    }

    private void PlaceTree(float _x, float _z)
    {
        Quaternion _rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        Instantiate(_boundaryTreePrefab, new Vector3(_x, 0f, _z), _rotation, BoundaryRoot);
    }
}
