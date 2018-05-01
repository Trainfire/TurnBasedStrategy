using UnityEngine;
using System;

public abstract class StateBase : MonoBehaviour
{
    public event Action StateRestored;
    public event Action StateSaved;
    public event Action StateCommitted;

    public event Action<StateID> Exited;

    public abstract StateID StateID { get; }
    public virtual StateFlags Flags { get; private set; }

    protected Gameboard Gameboard { get; private set; }
    protected StateEventsController Events { get; private set; }

    public void Initialize(Gameboard gameboard, StateEventsController gameboardEvents)
    {
        name = StateID.ToString();

        Gameboard = gameboard;
        Events = gameboardEvents;

        OnInitialize(gameboard, gameboardEvents);
    }

    protected virtual void OnInitialize(Gameboard gameboard, StateEventsController gameboardEvents) { }

    public void Enter()
    {
        Flags = new StateFlags();

        Debug.LogFormat("Enter state: [{0}]", StateID);
        OnEnter();
    }

    protected virtual void OnEnter() { }
    protected virtual void OnExit() { }

    protected void RestoreState() => StateRestored?.Invoke();
    protected void SaveState() => StateSaved?.Invoke();
    protected void CommitState() => StateCommitted?.Invoke();

    protected void ExitState()
    {
        Debug.LogFormat("Exit state: [{0}]", StateID);
        Flags.Clear();
        OnExit();
        Exited?.Invoke(StateID);
    }
}