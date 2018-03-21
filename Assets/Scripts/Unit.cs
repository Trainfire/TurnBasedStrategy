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

public class Unit : MonoBehaviour
{
    public event Action<UnitMoveEvent> Moved;
    public event Action<Unit> Died;

    public HealthComponent Health { get; private set; }
    public int MovementRange { get { return _unitData.MovementRange; } }

    [SerializeField] private UnitData _unitData;

    private GameboardHelper _gameboardHelper;

    public void Initialize(UnitData unitData, Tile targetTile, GameboardHelper gameboardHelper)
    {
        _unitData = unitData;
        _gameboardHelper = gameboardHelper;

        MoveTo(targetTile, true);

        Health = gameObject.GetOrAddComponent<HealthComponent>();
        Health.Initialize(unitData.MaxHealth);
        Health.Died += HealthComp_Died;
    }

    public bool MoveTo(Tile targetTile, bool ignoreDistance = false)
    {
        if (targetTile == null)
        {
            DebugEx.LogWarning<Unit>("Cannot move to a null tile.");
            return false;
        }

        if (!ignoreDistance && !_gameboardHelper.CanReachTile(transform.position.TransformToGridspace(), targetTile.Position, MovementRange))
            return false;

        if (!targetTile.Occupied)
        {
            Moved.InvokeSafe(new UnitMoveEvent(this, targetTile));
            targetTile.SetOccupant(this);
            return true;
        }

        return false;
    }

    public void Attack(Tile targetTile)
    {
        // TODO.
    }

    private void HealthComp_Died(HealthComponent healthComponent)
    {
        Died.InvokeSafe(this);
    }
}
