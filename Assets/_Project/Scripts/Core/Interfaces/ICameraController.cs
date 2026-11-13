using UnityEngine;

// 게임 카메라. 따라갈 대상과 움직일 수 있는 바닥 범위를 정한다.
public interface ICameraController
{
    // 시작할 때 이전 위치에서 미끄러져 오지 않고 바로 대상 위치에서 시작한다.
    void SetTarget(Transform target);

    // rect의 x는 월드 X, y는 월드 Z 범위다.
    void SetBounds(Rect rect);
}
