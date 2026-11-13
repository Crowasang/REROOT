using R3;

// 일시정지 창. 숲을 나가 강화하러 갈지, 계속 벨지 묻는다.
public interface IPauseView
{
    Observable<Unit> OnResume { get; }

    Observable<Unit> OnLeave { get; }

    void Show();

    void Hide();
}
