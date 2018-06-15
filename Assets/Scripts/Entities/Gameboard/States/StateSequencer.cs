using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Linq;
using System;
using Framework;

public interface IStateSequencer
{
    int TurnsLeft { get; }
}

public class StateSequencer : MonoBehaviour, IStateSequencer
{
    public StateBase Current { get { return _states[_sequence[_index]]; } }
    public int TurnCount { get; private set; }
    public int TurnsLeft { get { return Mathf.Max(0, _maxTurns - TurnCount); } }
    public IReadOnlyDictionary<StateID, StateBase> States { get { return _states; } }

    private int _index;
    private int _maxTurns;
    private List<StateID> _sequence = new List<StateID>();
    private Dictionary<StateID, StateBase> _states = new Dictionary<StateID, StateBase>();

    public void Initialize(Gameboard gameboard, StateEventsController eventsController)
    {
        _maxTurns = gameboard.Data.MaxTurns;

        AddState<StateSetupPhase>(gameboard, eventsController);
        AddState<StatePopulateWorld>(gameboard, eventsController);
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

            if (TurnCount == _maxTurns)
            {
                SetToEndGame();
                return;
            }

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
        _sequence.Add(StateID.PopulateWorld);
        _sequence.Add(StateID.TriggerSpawnPoints);
        _sequence.Add(StateID.EnemyThink);
        _sequence.Add(StateID.PlayerMove);
        _sequence.Add(StateID.EnemyAct);
    }

    private void SetToMainLoop()
    {
        Reset();

        _sequence.Add(StateID.PopulateWorld);
        _sequence.Add(StateID.EnemyThink);
        _sequence.Add(StateID.PlayerMove);
        _sequence.Add(StateID.EnemyAct);
        _sequence.Add(StateID.TriggerSpawnPoints);
    }

    public void SetToEndGame()
    {
        Reset();
        _sequence.Add(StateID.GameOver);
        _states[StateID.GameOver].Enter();
    }
}