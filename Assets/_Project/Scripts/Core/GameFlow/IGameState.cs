// 게임 씬 안의 한 단계(숲, 결과, 강화, 로딩…). 새 화면이 생기면 이걸 구현한 상태를 하나 추가하고
// GameLifetimeScope에 등록하면 된다. 상태끼리는 서로를 직접 부르지 않고 owner에게 전환을 요청한다.
public interface IGameState
{
    void Enter(IGameStateMachine owner);

    void Exit();

    // 매 프레임. 할 일이 없으면 비워 둔다.
    void Tick();
}
