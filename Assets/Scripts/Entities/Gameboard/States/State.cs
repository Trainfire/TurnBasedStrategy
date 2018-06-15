using Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class State : MonoBehaviour
{
    public IStateSequencer Sequencer { get { return _sequencer; } }
    public IStateEvents Events { get { return _eventsController; } }
    public IReadOnlyStateFlags Flags { get { return _sequencer.Current.Flags; } }

    private List<IStateHandler> _stateHandlers;
    private StateEventsController _eventsController;
    private StateSequencer _sequencer;

    private Gameboard _gameboard;

    public void Initialize(Gameboard gameboard)
    {
        _gameboard = gameboard;

        _eventsController = gameObject.AddComponent<StateEventsController>();

        _stateHandlers = new List<IStateHandler>();

        _gameboard.World.UnitAdded += RegisterUnit;
        _gameboard.World.Units.ToList().ForEach(x => RegisterUnit(x));
        _gameboard.World.Tiles.ToList().ForEach(stateHandler => _stateHandlers.Add(stateHandler.Value));

        _sequencer = GameObject.Instantiate(new GameObject("Sequencer"), transform).AddComponent<StateSequencer>();
        _sequencer.Initialize(_gameboard, _eventsController);

        _sequencer.States.Values.ToList().ForEach(state =>
        {
            state.StateRestored += OnRestoreState;
            state.StateSaved += OnSaveState;
            state.StateCommitted += OnCommitState;
        });

        _sequencer.Begin();
    }

    private void OnRestoreState() => _stateHandlers.ForEach(x => x.RestoreStateBeforeMove());
    private void OnSaveState() => _stateHandlers.ForEach(x => x.SaveStateBeforeMove());
    private void OnCommitState() => _stateHandlers.ForEach(x => x.CommitStateAfterAttack());

    private void OnBuildingHealthChanged(HealthChangeEvent healthChangeEvent)
    {
        if (_gameboard.World.Buildings.All(x => x.Health.Current == 0))
        {
            _sequencer.SetToEndGame();
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
        foreach (var state in _sequencer.States.Values)
        {
            state.StateRestored -= OnRestoreState;
            state.StateSaved -= OnSaveState;
            state.StateCommitted -= OnCommitState;
        }

        _stateHandlers.Clear();
    }
}