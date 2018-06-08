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
    public TileHazards Hazards { get; private set; }

    private string OccupantName { get { return Occupant != null ? Occupant.name : "Nobody"; } }

    private Helper _gameboardHelper;
    private List<IStateHandler> _stateHandlers;

    private void Awake()
    {
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

    public void ApplyHealthChange(int amount)
    {
        if (Occupant != null)
            Occupant.Health.Modify(amount);

        ReceivedHealthChange.InvokeSafe(this);
    }

    void IStateHandler.SaveStateBeforeMove() => _stateHandlers.ForEach(handler => handler.SaveStateBeforeMove());
    void IStateHandler.RestoreStateBeforeMove() => _stateHandlers.ForEach(handler => handler.RestoreStateBeforeMove());
    void IStateHandler.CommitStateAfterAttack() => _stateHandlers.ForEach(handler => handler.CommitStateAfterAttack());
}