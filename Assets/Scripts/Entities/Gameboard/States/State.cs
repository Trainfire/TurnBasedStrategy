using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Framework;

public enum StateID
{
    Invalid,
    Setup,
    PlayerMove,
    EnemyMove,
    GameOver,
}

public class State : MonoBehaviour
{
    public Tile CurrentSelection { get; private set; }

    public StateID Current { get; private set; }
    public IStateEvents Events { get { return _eventsController; } }
    public IReadOnlyStateFlags Flags { get { return _states[Current].Flags; } }
    public int TurnCount { get; private set; }

    private List<IStateHandler> _stateHandlers;
    private Dictionary<StateID, StateBase> _states;
    private StateEventsController _eventsController;

    private Gameboard _gameboard;

    public void Initialize(Gameboard gameboard)
    {
        _gameboard = gameboard;
        _gameboard.InputEvents.Select += OnPlayerInputSelect;

        _eventsController = gameObject.AddComponent<StateEventsController>();

        Register<StateSetupPhase>((state) => state.Initialize(_gameboard, _eventsController));
        Register<StatePlayerMovePhase>((state) => state.Initialize(_gameboard, _eventsController));
        Register<StateGameOver>((state) => state.Initialize(_gameboard, _eventsController));

        _stateHandlers = new List<IStateHandler>();

        _gameboard.World.UnitAdded += RegisterUnit;
        _gameboard.World.Units.ToList().ForEach(x => RegisterUnit(x));
        _gameboard.World.Tiles.ToList().ForEach(stateHandler => _stateHandlers.Add(stateHandler.Value));

        Current = StateID.Setup;

        _states[Current].Enter();
    }

    private void Register<TState>(Action<TState> postRegister) where TState : StateBase
    {
        if (_states == null)
            _states = new Dictionary<StateID, StateBase>();

        var instance = new GameObject().AddComponent<TState>();
        instance.transform.SetParent(transform);

        postRegister(instance);

        Assert.IsFalse(_states.ContainsKey(instance.StateID));

        _states.Add(instance.StateID, instance);

        instance.StateRestored += OnRestoreState;
        instance.StateSaved += OnSaveState;
        instance.StateCommitted += OnCommitState;
        instance.Exited += MoveNext;
    }

    private void OnRestoreState() => _stateHandlers.ForEach(x => x.RestoreStateBeforeMove());
    private void OnSaveState() => _stateHandlers.ForEach(x => x.SaveStateBeforeMove());
    private void OnCommitState() => _stateHandlers.ForEach(x => x.CommitStateAfterAttack());

    private void MoveNext(StateID previousStateID)
    {
        if (TurnCount == _gameboard.Data.MaxTurns)
        {
            _eventsController.EndGame(new GameEndedResult(GameEndedResultState.Win));
            return;
        }

        // TODO: Add logic for moving to other states.
        if (previousStateID == StateID.Setup)
            Current = StateID.PlayerMove;

        TurnCount++;

        Assert.IsTrue(_states.ContainsKey(Current), "No handler found for state.");

        _states[Current].Enter();
    }

    private void OnPlayerInputSelect(Tile selection)
    {
        if (selection == null)
            return;

        CurrentSelection = selection;

        DebugEx.Log<State>("Selected tile: " + CurrentSelection.name);
    }

    private void OnBuildingHealthChanged(HealthChangeEvent healthChangeEvent)
    {
        if (_gameboard.World.Buildings.All(x => x.Health.Current == 0))
        {
            Current = StateID.GameOver;
            _states[Current].Enter();
            _eventsController.EndGame(new GameEndedResult(GameEndedResultState.Loss));
        }
    }

    private void RegisterUnit(Unit unit)
    {
        _stateHandlers.Add(unit);

        if (unit.GetType() == typeof(Building))
            unit.Health.Changed += OnBuildingHealthChanged;
    }

    private void OnDestroy()
    {
        foreach (var state in _states.Values)
        {
            state.StateRestored -= OnRestoreState;
            state.StateSaved -= OnSaveState;
            state.StateCommitted -= OnCommitState;
            state.Exited -= MoveNext;
        }

        _stateHandlers.Clear();
    }
}