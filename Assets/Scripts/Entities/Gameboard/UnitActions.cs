using System;
using UnityEngine.Assertions;

public enum UnitAction
{
    Unassigned,
    Move,
    PrimaryAttack,
    SecondaryAttack,
}

public class UnitActionHandler
{
    private UnitAction _action;
    public UnitAction Action
    {
        get { return _action; }
        private set
        {
            _action = value;

            switch (_action)
            {
                case UnitAction.Move: _handler = new UnitMoveAction(); break;
                case UnitAction.PrimaryAttack: _handler = new UnitPrimaryAttackAction(); break;
                default: _handler = new UnitUnassignedAction(); break;
            }
        }
    }

    private IUnitAction _handler;
    public IUnitAction Handler { get { return _handler; } }

    private IStateEvents _stateEvents;

    public UnitActionHandler(IStateEvents stateEvents)
    {
        Action = UnitAction.Unassigned;

        _stateEvents = stateEvents;
        _stateEvents.ActionSetToAttack += StateEvents_ActionSetToAttack;
        _stateEvents.ActionCancelled += _stateEvents_ActionCancelled;
        _stateEvents.ActionSetToMove += _stateEvents_ActionSetToMove;
        _stateEvents.ActionCommitted += _stateEvents_ActionCommitted;
    }

    private void _stateEvents_ActionCommitted(StateActionCommittedEventArgs obj)
    {
        Action = UnitAction.Unassigned;
    }

    private void _stateEvents_ActionSetToMove(StateActionSetToMoveEventArgs obj)
    {
        Action = UnitAction.Move;
    }

    private void _stateEvents_ActionCancelled(StateActionCancelledEventArgs obj)
    {
        Action = UnitAction.Unassigned;
    }

    private void StateEvents_ActionSetToAttack(Mech obj)
    {
        Action = UnitAction.PrimaryAttack;
    }
}

public class UnitActionExecutionCompletedResult
{
    public UnitAction Action { get; private set; }

    public UnitActionExecutionCompletedResult(UnitAction action)
    {
        Action = action;
    }
}

public interface IUnitAction
{
    void Execute(Unit unit, Tile target, Action<UnitActionExecutionCompletedResult> onExecutionComplete);
    bool IsValid(Helper helper, Unit unit, Tile target);
}

public class UnitUnassignedAction : IUnitAction
{
    void IUnitAction.Execute(Unit unit, Tile target, Action<UnitActionExecutionCompletedResult> onExecutionComplete)
    {
        onExecutionComplete.Invoke(new UnitActionExecutionCompletedResult(UnitAction.Unassigned));
    }

    bool IUnitAction.IsValid(Helper helper, Unit unit, Tile target)
    {
        return false;
    }
}

// TODO: Make a MonoBehaviour so we can animate movement.
public class UnitMoveAction : IUnitAction
{
    public void Execute(Unit unit, Tile target, Action<UnitActionExecutionCompletedResult> onExecutionComplete)
    {
        unit.MoveTo(target);
        onExecutionComplete.Invoke(new UnitActionExecutionCompletedResult(UnitAction.Move));
    }

    bool IUnitAction.IsValid(Helper helper, Unit unit, Tile target)
    {
        var movementRange = unit is Mech ? (unit as Mech).MovementRange : helper.GridSize;
        return helper.CanReachTile(unit.Tile.transform.GetGridPosition(), target.transform.GetGridPosition(), movementRange);
    }
}

public class UnitPrimaryAttackAction : IUnitAction
{
    public void Execute(Unit unit, Tile target, Action<UnitActionExecutionCompletedResult> onExecutionComplete)
    {
        var weapon = unit.GetComponent<UnitWeaponComponent>();
        Assert.IsNotNull(weapon, "Cannot execute primary attack on a unit that doesn't have a primary weapon.");

        weapon.Use(target);

        onExecutionComplete.Invoke(new UnitActionExecutionCompletedResult(UnitAction.PrimaryAttack));
    }

    bool IUnitAction.IsValid(Helper helper, Unit unit, Tile target)
    {
        var weapon = unit.GetComponent<UnitWeaponComponent>();
        return weapon != null ? helper.CanAttackTile(unit, target, weapon.WeaponData) : false;
    }
}