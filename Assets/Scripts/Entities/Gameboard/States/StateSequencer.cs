using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Linq;
using System;
using Framework;

public class StateSequencer : MonoBehaviour
{
    public StateBase Current { get { return _states[_sequence[_index]]; } }
    public int TurnCount { get; private set; }
    public IReadOnlyDictionary<StateID, StateBase> States { get { return _states; } }

    private int _index;
    private List<StateID> _sequence = new List<StateID>();
    private Dictionary<StateID, StateBase> _states = new Dictionary<StateID, StateBase>();

    public void Initialize(Gameboard gameboard, StateEventsController eventsController)
    {
        AddState<StateSetupPhase>(gameboard, eventsController);
        AddState<StateBeginTurn>(gameboard, eventsController);
        AddState<StatePlayerMovePhase>(gameboard, eventsController);
        AddState<StateTriggerSpawnPoints>(gameboard, eventsController);
        AddState<StateEnemyThinkPhase>(gameboard, eventsController);
        AddState<StateEnemyActPhase>(gameboard, eventsController);
        AddState<StateGameOver>(gameboard, eventsController);

        SetToFirstTurnLoop();
    }

    private TState AddState<TState>(Gameboard gameboard, StateEventsController gameboardEvents) where TState : StateBase
    {
        var state = new GameObject().AddComponent<TState>();
        state.transform.SetParent(transform);
        state.Exited += OnStateExited;
        state.Initialize(gameboard, gameboardEvents);

        _states.Add(state.StateID, state);

        return state;
    }

    public void Begin()
    {
        _index = 0;
        _states[_sequence[_index]].Enter();
    }

    private void OnStateExited(StateID stateId)
    {
        MoveNext();
    }

    public void MoveNext()
    {
        if (_index == _sequence.Count - 1)
        {
            DebugEx.Log<StateSequencer>("End turn {0}", TurnCount + 1);

            TurnCount++;

            if (TurnCount != 0)
                SetToMainLoop();

            DebugEx.Log<State>("Start turn {0}", TurnCount + 1);

            Begin();
        }
        else
        {
            _index++;
            _states[_sequence[_index]].Enter();
        }
    }

    private void Reset()
    {
        _index = 0;
        _sequence.Clear();
    }

    private void SetToFirstTurnLoop()
    {
        Reset();

        _sequence.Add(StateID.Setup);
        _sequence.Add(StateID.BeginTurn);
        _sequence.Add(StateID.TriggerSpawnPoints);
        _sequence.Add(StateID.EnemyThink);
        _sequence.Add(StateID.EnemyAct);
        _sequence.Add(StateID.PlayerMove);
    }

    private void SetToMainLoop()
    {
        Reset();

        _sequence.Add(StateID.BeginTurn);
        _sequence.Add(StateID.EnemyThink);
        _sequence.Add(StateID.PlayerMove);
        _sequence.Add(StateID.TriggerSpawnPoints);
        _sequence.Add(StateID.EnemyAct);
    }

    public void SetToEndGame()
    {
        Reset();

        _states[StateID.GameOver].Enter();
    }
}