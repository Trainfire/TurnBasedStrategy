using UnityEngine;
using UnityEngine.Assertions;
using System;
using Framework;

public struct UnitMoveEvent
{
    public Unit Unit { get; private set; }
    public Tile TargetTile { get; private set; }

    public UnitMoveEvent(Unit unit, Tile target)
    {
        Unit = unit;
        TargetTile = target;
    }
}

public class Unit : MonoBehaviour, IStateHandler
{
    public event Action<UnitMoveEvent> Moved;
    public event Action<Unit> Removed;

    public Tile Tile { get { return _helper.GetTile(this); } }
    public HealthComponent Health { get; private set; }
    public Helper Helper { get { return _helper; } }

    private Helper _helper;

    private void Awake()
    {
        Health = gameObject.AddComponent<HealthComponent>();
    }

    public virtual void Initialize(Helper helper)
    {
        _helper = helper;
        name = "Unnamed Unit";
    }

    protected T AddUnitComponent<T>() where T : UnitComponent
    {
        var component = gameObject.AddComponent<T>();
        component.Initialize(this);
        return component;
    }

    public bool MoveTo(Tile targetTile)
    {
        if (targetTile == null)
        {
            DebugEx.LogWarning<Unit>("Cannot move to a null tile.");
            return false;
        }

        if (!targetTile.Blocked)
        {
            Moved.InvokeSafe(new UnitMoveEvent(this, targetTile));
            //targetTile.SetOccupant(this);
            return true;
        }

        return false;
    }

    public bool MoveInDirection(WorldDirection worldDirection)
    {
        return MoveTo(_helper.GetTileInDirection(this, worldDirection));
    }

    protected void RemoveSelf()
    {
        Removed.InvokeSafe(this);
        Destroy(gameObject);
    }

    protected void SetName(string name)
    {
        if (name != null && name != string.Empty)
            this.name = name;
    }

    public void SaveStateBeforeMove()
    {
        ((IStateHandler)Health).SaveStateBeforeMove();
    }

    public void RestoreStateBeforeMove()
    {
        ((IStateHandler)Health).RestoreStateBeforeMove();
    }

    public void CommitStateAfterAttack()
    {
        ((IStateHandler)Health).CommitStateAfterAttack();
    }
}
