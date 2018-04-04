using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using Framework;

public class Tile : MonoBehaviour, IStateHandler
{
    public event Action<Tile> OccupantLeft;
    public event Action<Tile> OccupantEntered;
    public event Action<Tile> ReceivedHealthChange;

    public bool Blocked { get { return _occupant != null; } }

    public TileMarker Marker { get; private set; }
    public Unit Occupant { get { return _occupant; } }
    public List<TileHazard> Hazards { get { return _hazards; } }

    private string OccupantName { get { return _occupant != null ? _occupant.name : "Nobody"; } }

    private GameboardWorldHelper _gameboardHelper;
    private Unit _occupant;
    private List<TileHazard> _hazards;
    private List<IStateHandler> _stateHandlers;

    private void Awake()
    {
        Marker = gameObject.GetOrAddComponent<TileMarker>();
        _hazards = new List<TileHazard>();
        _stateHandlers = new List<IStateHandler>();
    }

    public void Initialize(GameboardWorldHelper gameboardHelper)
    {
        _gameboardHelper = gameboardHelper;
    }

    public void SetOccupant(Unit occupant)
    {
        if (_occupant != null)
        {
            _occupant.Removed -= OnOccupantDeath;
            _occupant.Moved -= OnOccupantMoved;

            OccupantLeft.InvokeSafe(this);
        }

        _occupant = occupant;

        Debug.LogFormat("{0} is now occupied by {1}", name, OccupantName);

        if (_occupant != null)
        {
            _occupant.Removed += OnOccupantDeath;
            _occupant.Moved += OnOccupantMoved;
            _occupant.transform.SetGridPosition(transform.GetGridPosition());

            OccupantEntered.InvokeSafe(this);
        }
    }

    public void AddHazard<T>(T prototype) where T : TileHazard
    {
        DebugEx.Log<Tile>("Added hazard: {0}", typeof(T).Name);

        var hazard = Instantiate(prototype);
        hazard.Initialize(this, _gameboardHelper);
        hazard.transform.SetGridPosition(transform.GetGridPosition());

        _hazards.Add(hazard);

        var stateHandler = hazard as IStateHandler;
        if (stateHandler != null)
            _stateHandlers.Add(stateHandler);
    }

    public void RemoveHazard(TileHazard hazard)
    {
        Assert.IsTrue(_hazards.Contains(hazard));

        DebugEx.Log<Tile>("Removed hazard: {0}", hazard.name);

        _hazards.Remove(hazard);
    }

    public void ApplyHealthChange(int amount)
    {
        if (Occupant != null)
            Occupant.Health.Modify(amount);

        ReceivedHealthChange.InvokeSafe(this);
    }

    private void OnOccupantDeath(Unit unit)
    {
        SetOccupant(null);
    }

    private void OnOccupantMoved(UnitMoveEvent unitMoveEvent)
    {
        Debug.LogFormat("Occupant {0} vacated from {1} ", OccupantName, name);
        SetOccupant(null);
    }

    void IStateHandler.SaveStateBeforeMove() => _stateHandlers.ForEach(handler => handler.SaveStateBeforeMove());
    void IStateHandler.RestoreStateBeforeMove() => _stateHandlers.ForEach(handler => handler.RestoreStateBeforeMove());
    void IStateHandler.CommitStateAfterAttack() => _stateHandlers.ForEach(handler => handler.CommitStateAfterAttack());
}
