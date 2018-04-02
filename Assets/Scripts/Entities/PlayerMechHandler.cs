using UnityEngine;
using UnityEngine.Assertions;
using System;
using Framework;

public struct PlayerMechHandlerActionSelectedEvent
{
    public PlayerMechHandler Sender { get; private set; }
    public Mech Mech { get; private set; }
    public UnitActionType ActionType { get; private set; }

    public PlayerMechHandlerActionSelectedEvent(PlayerMechHandler mechHandler, Mech mech, UnitActionType actionType)
    {
        Sender = mechHandler;
        Mech = mech;
        ActionType = actionType;
    }
}

public struct PlayerMechHandlerActionCommittedEvent
{
    public PlayerMechHandler Sender { get; private set; }
    public UnitActionType ActionType { get; private set; }

    public PlayerMechHandlerActionCommittedEvent(PlayerMechHandler mechHandler, UnitActionType actionType)
    {
        Sender = mechHandler;
        ActionType = actionType;
    }
}

public class PlayerMechHandler
{
    public event Action<PlayerMechHandlerActionCommittedEvent> ActionCommitted;
    public event Action<PlayerMechHandlerActionSelectedEvent> ActionSelected;

    public UnitActionType CurrentAction { get; private set; }

    private Mech _mech;
    private GameboardWorldHelper _gameboardWorldHelper;
    private GameboardVisualizer _gameboardVisualizer;
    private GameboardState _gameboardState;

    public PlayerMechHandler(GameboardWorldHelper gameboardWorldHelper, GameboardVisualizer gameboardVisualizer, GameboardState gameboardState)
    {
        Assert.IsNotNull(gameboardWorldHelper);
        Assert.IsNotNull(gameboardVisualizer);
        Assert.IsNotNull(gameboardState);

        _gameboardWorldHelper = gameboardWorldHelper;
        _gameboardVisualizer = gameboardVisualizer;
        _gameboardState = gameboardState;
    }

    public void Set(Mech mech)
    {
        _mech = mech;
    }

    public void Clear()
    {
        _mech = null;
    }

    public void SetCurrentAction(UnitActionType actionType)
    {
        if (_mech == null || !_gameboardState.ValidPlayerActions.CanControlUnits)
            return;

        CurrentAction = actionType;

        if (CurrentAction == UnitActionType.Move)
            _gameboardVisualizer.ShowReachablePositions(_mech);

        if (CurrentAction == UnitActionType.AttackPrimary)
        {
            if (_mech.PrimaryWeapon != null)
                _gameboardVisualizer.ShowTargetableTiles(_mech, _mech.PrimaryWeapon.WeaponData);
        }

        ActionSelected.InvokeSafe(new PlayerMechHandlerActionSelectedEvent(this, _mech, actionType));

        // TODO: Support for AttackSecondary.
    }

    public void CommitCurrentAction(Tile targetTile)
    {
        if (_mech == null || CurrentAction == UnitActionType.Unassigned || targetTile == null)
            return;

        if (!_gameboardState.ValidPlayerActions.CanControlUnits)
        {
            DebugEx.LogWarning<Player>("Cannot commit action whilst 'CanControlUnits' is false.");
            return;
        }

        // TEMP: Eventually We'll have callbacks to know when the action has finished completing.
        var actionComplete = false;

        if (CurrentAction == UnitActionType.Move)
        {
            if (_gameboardWorldHelper.CanReachTile(_mech.transform.GetGridPosition(), targetTile.transform.GetGridPosition(), _mech.MovementRange))
            {
                _mech.MoveTo(targetTile);
                actionComplete = true;
            }
        }

        if (CurrentAction == UnitActionType.AttackPrimary)
        {
            _mech.PrimaryWeapon.Use(targetTile);
            actionComplete = true;
        }

        if (actionComplete)
        {
            DebugEx.Log<Player>("Commit action '{0}' on tile {1} using unit {2}", CurrentAction, targetTile.transform.GetGridPosition(), _mech.name);
            ActionCommitted.InvokeSafe(new PlayerMechHandlerActionCommittedEvent(this, CurrentAction));
        }
    }
}
