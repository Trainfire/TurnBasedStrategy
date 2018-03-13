using System;
using System.Collections.Generic;

namespace Framework
{
    public enum State
    {
        Running,
        Paused,
    }

    public interface IStateHandler
    {
        event Action<State> StateChanged;
        void OnStateChanged(State state);
    }

    public class StateManager 
    {
        private IStateHandler _handler;

        public StateListener Listener { get; private set; }
        public State State { get; private set; }

        public StateManager()
        {
            Listener = new StateListener();
            _handler = Listener;
        }

        public void SetState(State state)
        {
            State = state;
            _handler.OnStateChanged(State);
        }

        public void ToggleState()
        {
            SetState(State == State.Paused ? State.Running : State.Paused);
        }
    }

    public class StateListener : IStateHandler
    {
        public event Action<State> StateChanged;

        void IStateHandler.OnStateChanged(State state)
        {
            if (StateChanged != null)
                StateChanged(state);
        }
    }
}
