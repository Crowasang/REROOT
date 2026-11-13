using R3;

// 게임 흐름. 지금 어느 단계인지 알리고, 단계를 바꾼다.
public interface IGameStateMachine
{
    // 화면, 사운드, 디버그 표시 등이 현재 단계에 반응할 때 구독한다.
    ReadOnlyReactiveProperty<IGameState> Current { get; }

    void ChangeState<TState>() where TState : IGameState;
}
