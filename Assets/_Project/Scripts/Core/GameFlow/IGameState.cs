// 게임 씬 안의 한 단계(숲, 결과, 강화, 로딩…). 새 화면이 생기면 이걸 구현한 상태를 하나 추가하고
// GameLifetimeScope에 등록하면 된다. 상태끼리는 서로를 직접 부르지 않고 owner에게 전환을 요청한다.
// 매 프레임 확인하지 않는다: Enter에서 이벤트(R3)를 구독하거나 UniTask로 기다리고, Exit에서 정리한다.
public interface IGameState
{
    void Enter(IGameStateMachine _owner);

    void Exit();
}
