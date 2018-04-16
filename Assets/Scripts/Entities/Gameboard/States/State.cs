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
    public ReadOnlyStateFlags Flags { get { return _states[Current].Flags.AsReadOnly(); } }
    public int TurnCount { get; private set; }

    private Dictionary<StateID, StateBase> _states;
    private StateEventsController _eventsController;

    private Gameboard _gameboard;

    public void Initialize(Gameboard gameboard)
    {
        _gameboard = gameboard;
        _gameboard.InputEvents.Select += OnPlayerInputSelect;

        _eventsController = gameObject.AddComponent<StateEventsController>();

        Register(new StateSetupPhase(_gameboard, _eventsController));
        Register(new StatePlayerMovePhase(_gameboard, _eventsController));
        Register(new StateGameOver(_gameboard, _eventsController));

        foreach (var unit in _gameboard.Entities.Units.ToList())
        {
            if (unit.GetType() == typeof(Building))
                unit.Health.Changed += OnBuildingHealthChanged;
        }

        Current = StateID.Setup;

        _states[Current].Enter();
    }

    private void Register(StateBase gameboardState)
    {
        if (_states == null)
            _states = new Dictionary<StateID, StateBase>();

        Assert.IsFalse(_states.ContainsKey(gameboardState.StateID));

        _states.Add(gameboardState.StateID, gameboardState);

        gameboardState.Exited += MoveNext;
    }

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
        if (_gameboard.Entities.Buildings.All(x => x.Health.Current == 0))
        {
            Current = StateID.GameOver;
            _states[Current].Enter();
            _eventsController.EndGame(new GameEndedResult(GameEndedResultState.Loss));
        }
    }
}