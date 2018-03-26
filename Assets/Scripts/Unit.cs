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

public struct UnitPushbackEvent
{
    public Unit Unit { get; private set; }
    public WorldDirection Direction { get; private set; }

    public UnitPushbackEvent(Unit unit, WorldDirection direction)
    {
        Unit = unit;
        Direction = direction;
    }
}

public class Unit : MonoBehaviour
{
    public event Action<UnitMoveEvent> Moved;
    public event Action<Unit> Died;

    public Vector2 Position { get { return transform.position.TransformToGridspace(); } }
    public HealthComponent Health { get; private set; }
    public int MovementRange { get { return _unitData.MovementRange; } }
    public WeaponData PrimaryWeapon { get { return _primaryWeaponData; } }

    [SerializeField] private UnitData _unitData;
    [SerializeField] private WeaponData _primaryWeaponData;

    private GameboardHelper _gameboardHelper;

    public void Initialize(UnitData unitData, Tile targetTile, GameboardHelper gameboardHelper)
    {
        _unitData = unitData;
        _primaryWeaponData = _unitData.DefaultPrimaryWeapon;
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

    public bool MoveTo(WorldDirection worldDirection)
    {
        return MoveTo(_gameboardHelper.GetTileInDirection(this, worldDirection));
    }

    public void Push(WorldDirection worldDirection)
    {
        var pushbackResults = _gameboardHelper.GetPushbackResults(this, worldDirection);

        // Push unit if no other units will be hit.
        // Otherwise apply 1 damage to each unit.
        if (pushbackResults.Count == 1)
        {
            Assert.IsTrue(pushbackResults[0].Unit == this);
            MoveTo(worldDirection);
        }
        else
        {
            foreach (var pushbackResult in pushbackResults)
            {
                pushbackResult.Unit.Health.Modify(-1);

                if (_gameboardHelper.CanReachTile(pushbackResult.Unit.Position, worldDirection))
                    pushbackResult.Unit.MoveTo(worldDirection);
            }
        }
    }

    public bool Attack(Tile targetTile)
    {
        Assert.IsNotNull(_primaryWeaponData, "Primary weapon data is missing.");
        Assert.IsNotNull(_primaryWeaponData.EffectPrototype, "Effect prototype is missing from primary weapon data.");

        if (!_gameboardHelper.CanAttackTile(this, targetTile, _primaryWeaponData))
            return false;

        var effectInstance = Instantiate<EffectRoot>(_primaryWeaponData.EffectPrototype);
        effectInstance.Apply(_gameboardHelper, new UnitAttackEvent(this, targetTile, _primaryWeaponData));

        return true;
    }

    private void HealthComp_Died(HealthComponent healthComponent)
    {
        Died.InvokeSafe(this);
    }
}
