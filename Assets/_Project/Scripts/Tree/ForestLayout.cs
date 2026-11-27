using System.Collections.Generic;
using UnityEngine;

// 나무를 놓을 자리를 계산한다. 오브젝트를 만들지 않는 순수 계산이라 배치 방식을 바꾸거나 시험하기 쉽다.
public static class ForestLayout
{
    // 바닥(XZ) 영역에 일정 간격 격자를 만들고, 남는 여백은 양쪽에 똑같이 나눈다.
    // clearCenter 주변 clearRadius 안은 비워 둔다(플레이어 시작 자리).
    public static void FillGrid(
        List<Pose> _results,
        Bounds _area,
        float _spacing,
        float _jitter,
        bool _randomYaw,
        Vector3? _clearCenter,
        float _clearRadius)
    {
        _results.Clear();

        int _columns = Mathf.FloorToInt(_area.size.x / _spacing) + 1;
        int _rows = Mathf.FloorToInt(_area.size.z / _spacing) + 1;
        Vector3 _origin = new(
            _area.center.x - (_columns - 1) * _spacing * 0.5f,
            0f,
            _area.center.z - (_rows - 1) * _spacing * 0.5f);

        float _clearSqr = _clearRadius * _clearRadius;

        for (int _z = 0; _z < _rows; _z++)
        {
            for (int _x = 0; _x < _columns; _x++)
            {
                Vector3 _position = _origin + new Vector3(_x, 0f, _z) * _spacing;

                if (_clearCenter.HasValue && FlatSqrDistance(_clearCenter.Value, _position) < _clearSqr)
                {
                    continue;
                }

                Vector2 _offset = Random.insideUnitCircle * _jitter;
                _position += new Vector3(_offset.x, 0f, _offset.y);

                // 같은 모델이 똑같은 방향으로 늘어서 보이지 않도록 제자리에서 돌린다.
                Quaternion _rotation = _randomYaw ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) : Quaternion.identity;
                _results.Add(new Pose(_position, _rotation));
            }
        }
    }

    // 높이는 무시하고 바닥 위 거리로만 잰다.
    private static float FlatSqrDistance(Vector3 _a, Vector3 _b)
    {
        float _dx = _a.x - _b.x;
        float _dz = _a.z - _b.z;
        return _dx * _dx + _dz * _dz;
    }
}
