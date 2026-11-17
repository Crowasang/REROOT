using R3;

// 세션 중 화면 위에 늘 떠 있는 정보(남은 시간, 재화)와 일시정지 버튼
public interface IHudView
{
    Observable<Unit> OnPauseClicked { get; }

    // ratio: 시작 시간 대비 남은 시간(0~1)
    void SetTime(float ratio, int seconds);

    void SetWood(int wood);
}
