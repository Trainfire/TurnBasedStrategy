using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using Framework;
using System.Linq;

public class Tile : MonoBehaviour, IStateHandler
{
    public event Action<Tile> OccupantLeft;
    public event Action<Tile> OccupantEntered;
    public event Action<Tile> ReceivedHealthChange;

    public bool Blocked { get { return Occupant != null; } }

    public Unit Occupant { get { return _gameboardHelper.GetUnit(this); } }
    public TileMarker Marker { get; private set; }
    public TileHazards Hazards { get; private set; }

    private string OccupantName { get { return Occupant != null ? Occupant.name : "Nobody"; } }

    private Helper _gameboardHelper;
    private List<IStateHandler> _stateHandlers;

    private void Awake()
    {
        Marker = gameObject.GetOrAddComponent<TileMarker>();

        _stateHandlers = new List<IStateHandler>();
    }

    public void Initialize(Helper gameboardHelper)
    {
        _gameboardHelper = gameboardHelper;

        Hazards = gameObject.AddComponent<TileHazards>();
        Hazards.Initialize(this, _gameboardHelper);

        _stateHandlers.Add(Hazards);
    }

    public void Enter(Unit unit)
    {
        OccupantEntered.InvokeSafe(this);
    }

    public void Leave(Unit unit)
    {
        OccupantLeft.InvokeSafe(this);
    }

    //public void SetOccupant(Unit newOccupant)
    //{
    //    if (_occupant.Previous != null)
    //    {
    //        //Occupant.Removed -= OnOccupantDeath;
    //        //Occupant.Moved -= OnOccupantMoved;

    //        OccupantLeft.InvokeSafe(this);
    //    }

    //    _occupant.Current = newOccupant;

    //    //OccupantChanged?.Invoke(this, newOccupant);

    //    Debug.LogFormat("{0} is now occupied by {1}", name, OccupantName);

    //    if (_occupant.Current != null)
    //    {
    //        //Occupant.Removed += OnOccupantDeath;
    //        //Occupant.Moved += OnOccupantMoved;
    //        _occupant.Current.transform.SetGridPosition(transform.GetGridPosition());

    //        OccupantEntered.InvokeSafe(this);
    //    }
    //}

    public void ApplyHealthChange(int amount)
    {
        if (Occupant != null)
            Occupant.Health.Modify(amount);

        ReceivedHealthChange.InvokeSafe(this);
    }

    //private void RemoveOccupant()
    //{
    //    OccupantLeft.InvokeSafe(this);
    //}

    //private void OnOccupantDeath(Unit unit)
    //{
    //    RemoveOccupant();
    //}

    //private void OnOccupantMoved(UnitMoveEvent unitMoveEvent)
    //{
    //    Debug.LogFormat("Occupant {0} vacated from {1} ", OccupantName, name);
    //    RemoveOccupant();
    //}

    void IStateHandler.SaveStateBeforeMove() => _stateHandlers.ForEach(handler => handler.SaveStateBeforeMove());
    void IStateHandler.RestoreStateBeforeMove() => _stateHandlers.ForEach(handler => handler.RestoreStateBeforeMove());
    void IStateHandler.CommitStateAfterAttack() => _stateHandlers.ForEach(handler => handler.CommitStateAfterAttack());
}