using R3;

// 세션이 끝나면 뜨는 결과 창. 강화 화면이 생기면 여기서 넘어간다.
public interface ISessionEndView
{
    Observable<Unit> OnContinue { get; }

    void Show(SessionResult result);

    void Hide();
}
