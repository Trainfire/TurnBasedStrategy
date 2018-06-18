using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class UnitActionExecutionCompletedResult
{
    public Unit Actor { get; private set; }
    public UnitAction Action { get; private set; }

    public UnitActionExecutionCompletedResult(Unit actor, UnitAction action)
    {
        Actor = actor;
        Action = action;
    }
}

public interface IUnitAction
{
    void Execute(Helper helper, Unit unit, Tile target, Action<UnitActionExecutionCompletedResult> onExecutionComplete);
    bool IsValid(Helper helper, Unit unit, Tile target);
}

public class UnitUnassignedAction : IUnitAction
{
    void IUnitAction.Execute(Helper helper, Unit unit, Tile target, Action<UnitActionExecutionCompletedResult> onExecutionComplete)
    {
        onExecutionComplete.Invoke(new UnitActionExecutionCompletedResult(unit, UnitAction.Unassigned));
    }

    bool IUnitAction.IsValid(Helper helper, Unit unit, Tile target)
    {
        return false;
    }
}

// TODO: Make a MonoBehaviour so we can animate movement.
public class UnitMoveAction : IUnitAction
{
    public void Execute(Helper helper, Unit unit, Tile target, Action<UnitActionExecutionCompletedResult> onExecutionComplete)
    {
        var animator = new GameObject().AddComponent<UnitMoveAnimator>();
        animator.Animate(helper, unit, target, () =>
        {
            unit.MoveTo(target);
            onExecutionComplete.Invoke(new UnitActionExecutionCompletedResult(unit, UnitAction.Move));
        });
    }

    bool IUnitAction.IsValid(Helper helper, Unit unit, Tile target)
    {
        var movementRange = unit is Mech ? (unit as Mech).MovementRange : helper.GridSize;
        return helper.CanReachTile(unit.Tile.transform.GetGridPosition(), target.transform.GetGridPosition(), movementRange);
    }
}

public class UnitPrimaryAttackAction : IUnitAction
{
    public void Execute(Helper helper, Unit unit, Tile target, Action<UnitActionExecutionCompletedResult> onExecutionComplete)
    {
        var weapon = unit.GetComponent<UnitWeaponComponent>();
        Assert.IsNotNull(weapon, "Cannot execute primary attack on a unit that doesn't have a primary weapon.");

        weapon.Use(target);

        onExecutionComplete.Invoke(new UnitActionExecutionCompletedResult(unit, UnitAction.PrimaryAttack));
    }

    bool IUnitAction.IsValid(Helper helper, Unit unit, Tile target)
    {
        var weapon = unit.GetComponent<UnitWeaponComponent>();
        return weapon != null ? helper.CanAttackTile(unit, target, weapon.WeaponData) : false;
    }
}

public class UnitRepairAction : IUnitAction
{
    public void Execute(Helper helper, Unit unit, Tile target, Action<UnitActionExecutionCompletedResult> onExecutionComplete)
    {
        unit.Health.Modify(1);
        onExecutionComplete.Invoke(new UnitActionExecutionCompletedResult(unit, UnitAction.PrimaryAttack));
    }

    bool IUnitAction.IsValid(Helper helper, Unit unit, Tile target)
    {
        return unit.Health.Current > 0 && unit.Health.Current < unit.Health.Max;
    }
}