using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

// 게임 흐름 상태 머신(상태 패턴). 등록된 상태들을 받아 두고 하나만 실행한다.
// 상태 객체는 컨테이너가 만들어 주므로 각 상태는 필요한 서비스를 생성자로 받는다.
// 매 프레임 도는 일은 없다. 각 상태는 Enter에서 필요한 이벤트를 구독하고 Exit에서 끊는다.
public sealed class GameStateMachine : IGameStateMachine, IDisposable
{
    private readonly Dictionary<Type, IGameState> _states = new();
    private readonly ReactiveProperty<IGameState> _current = new(null);
    private bool _isChanging;

    public GameStateMachine(IReadOnlyList<IGameState> _states)
    {
        foreach (IGameState _state in _states)
        {
            this._states[_state.GetType()] = _state;
        }
    }

    public ReadOnlyReactiveProperty<IGameState> Current => _current;

    public void ChangeState<TState>() where TState : IGameState
    {
        if (!_states.TryGetValue(typeof(TState), out IGameState _next))
        {
            Debug.LogError($"[GameStateMachine] 등록되지 않은 상태: {typeof(TState).Name}. GameLifetimeScope에 등록했는지 확인하세요.");
            return;
        }

        // Exit/Enter 안에서 또 전환을 요청하면 순서가 꼬이므로 막는다.
        if (_isChanging)
        {
            Debug.LogError($"[GameStateMachine] 상태 전환 중에 {typeof(TState).Name}(으)로의 전환을 요청했습니다.");
            return;
        }

        _isChanging = true;
        try
        {
            _current.Value?.Exit();
            _current.Value = _next;
            _next.Enter(this);
        }
        finally
        {
            _isChanging = false;
        }
    }

    public void Dispose()
    {
        _current.Value?.Exit();
        _current.Dispose();
    }
}
