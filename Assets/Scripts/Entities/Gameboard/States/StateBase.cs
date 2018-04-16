using UnityEngine;
using System;

public abstract class StateBase
{
    public event Action<StateID> Exited;

    public abstract StateID StateID { get; }

    public virtual bool CanExitState { get { return true; } }
    public virtual StateFlags Flags { get; private set; }

    protected Gameboard Gameboard { get; private set; }
    protected StateEventsController Events { get; private set; }

    public StateBase(Gameboard gameboard, StateEventsController gameboardEvents)
    {
        Gameboard = gameboard;
        Events = gameboardEvents;
    }

    public void Enter()
    {
        Flags = new StateFlags();

        Debug.LogFormat("Enter state: [{0}]", StateID);
        OnEnter();
    }

    protected virtual void OnEnter() { }

    protected void ExitState()
    {
        Debug.LogFormat("Exit state: [{0}]", StateID);
        Flags.Clear();
        Exited?.Invoke(StateID);
    }
}