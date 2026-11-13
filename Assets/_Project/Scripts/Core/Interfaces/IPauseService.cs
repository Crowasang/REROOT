using R3;

// 게임 일시정지. 세션 중에만 멈출 수 있다.
public interface IPauseService
{
    ReadOnlyReactiveProperty<bool> IsPaused { get; }

    void Pause();

    void Resume();
}
