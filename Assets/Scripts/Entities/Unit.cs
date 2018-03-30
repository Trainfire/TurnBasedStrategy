using UnityEngine;
using UnityEngine.Assertions;
using System;
using Framework;

public struct UnitAttackEvent
{
    public Unit Source { get; private set; }
    public Tile TargetTile { get; private set; }
    public WeaponData WeaponData { get; private set; }

    public UnitAttackEvent(Unit source, Tile targetTile, WeaponData weaponData)
    {
        Source = source;
        TargetTile = targetTile;
        WeaponData = weaponData;
    }
}

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
    public event Action<Unit> Removed;

    public HealthComponent Health { get; private set; }
    public GameboardHelper Helper { get { return _gameboardHelper; } }

    private GameboardHelper _gameboardHelper;

    private void Awake()
    {
        Health = gameObject.AddComponent<HealthComponent>();
        Health.Killed += OnKill;
    }

    protected void Initialize(GameboardHelper gameboardHelper)
    {
        _gameboardHelper = gameboardHelper;
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

        if (!targetTile.Occupied)
        {
            Moved.InvokeSafe(new UnitMoveEvent(this, targetTile));
            targetTile.SetOccupant(this);
            return true;
        }

        return false;
    }

    public bool MoveInDirection(WorldDirection worldDirection)
    {
        return MoveTo(_gameboardHelper.GetTileInDirection(this, worldDirection));
    }

    public void Remove()
    {
        Removed.InvokeSafe(this);
        Destroy(gameObject);
    }

    protected void SetName(string name)
    {
        if (name != null && name != string.Empty)
            this.name = name;
    }

    protected virtual void OnKill(HealthComponent healthComponent)
    {
        Remove();
    }
}
