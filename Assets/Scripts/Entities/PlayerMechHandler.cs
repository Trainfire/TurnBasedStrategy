using UnityEngine;
using UnityEngine.Assertions;
using System;
using Framework;

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

    public UnitActionType CurrentAction { get; private set; }

    private Mech _mech;
    private Player _player;

    public PlayerMechHandler(Player player)
    {
        Assert.IsNotNull(player.Input);

        _player = player;
        _player.Input.CommitCurrentAction += PlayerInput_CommitCurrentAction;
        _player.Input.SetCurrentAction += PlayerInput_SetCurrentAction;
    }

    public void Set(Mech mech)
    {
        _mech = mech;
    }

    public void Clear()
    {
        _mech = null;
    }

    private void PlayerInput_SetCurrentAction(UnitActionType actionType)
    {
        if (_mech == null)
            return;

        CurrentAction = actionType;

        if (CurrentAction == UnitActionType.Move)
            _player.Gameboard.Visualizer.ShowReachablePositions(_mech);

        if (CurrentAction == UnitActionType.AttackPrimary)
        {
            if (_mech.PrimaryWeapon != null)
                _player.Gameboard.Visualizer.ShowTargetableTiles(_mech, _mech.PrimaryWeapon.WeaponData);
        }

        // TODO: Support for AttackSecondary.
    }

    private void PlayerInput_CommitCurrentAction(Tile targetTile)
    {
        if (_mech == null || CurrentAction == UnitActionType.Unassigned || targetTile == null)
            return;

        // TEMP: Eventually We'll have callbacks to know when the action has finished completing.
        var actionComplete = false;

        if (CurrentAction == UnitActionType.Move)
        {
            if (_player.Gameboard.Helper.CanReachTile(_mech.transform.GetGridPosition(), targetTile.transform.GetGridPosition(), _mech.MovementRange))
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
