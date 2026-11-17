using UnityEngine;

// 벌목 영역을 둘러싼 맵 경계
public interface IMapBoundary
{
    // 카메라가 따라갈 수 있는 바닥 범위. x는 월드 X, y는 월드 Z다. Build() 뒤에 유효하다.
    Rect CameraRect { get; }

    void Build();
}
