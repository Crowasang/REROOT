using System.Collections.Generic;
using UnityEngine;

// 나무를 놓을 자리를 계산한다. 오브젝트를 만들지 않는 순수 계산이라 배치 방식을 바꾸거나 시험하기 쉽다.
public static class ForestLayout
{
    // 바닥(XZ) 영역에 일정 간격 격자를 만들고, 남는 여백은 양쪽에 똑같이 나눈다.
    // clearCenter 주변 clearRadius 안은 비워 둔다(플레이어 시작 자리).
    public static void FillGrid(
        List<Pose> results,
        Bounds area,
        float spacing,
        float jitter,
        bool randomYaw,
        Vector3? clearCenter,
        float clearRadius)
    {
        results.Clear();

        int columns = Mathf.FloorToInt(area.size.x / spacing) + 1;
        int rows = Mathf.FloorToInt(area.size.z / spacing) + 1;
        Vector3 origin = new(
            area.center.x - (columns - 1) * spacing * 0.5f,
            0f,
            area.center.z - (rows - 1) * spacing * 0.5f);

        float clearSqr = clearRadius * clearRadius;

        for (int z = 0; z < rows; z++)
        {
            for (int x = 0; x < columns; x++)
            {
                Vector3 position = origin + new Vector3(x, 0f, z) * spacing;

                if (clearCenter.HasValue && FlatSqrDistance(clearCenter.Value, position) < clearSqr)
                {
                    continue;
                }

                Vector2 offset = Random.insideUnitCircle * jitter;
                position += new Vector3(offset.x, 0f, offset.y);

                // 같은 모델이 똑같은 방향으로 늘어서 보이지 않도록 제자리에서 돌린다.
                Quaternion rotation = randomYaw ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) : Quaternion.identity;
                results.Add(new Pose(position, rotation));
            }
        }
    }

    // 높이는 무시하고 바닥 위 거리로만 잰다.
    private static float FlatSqrDistance(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return dx * dx + dz * dz;
    }
}
