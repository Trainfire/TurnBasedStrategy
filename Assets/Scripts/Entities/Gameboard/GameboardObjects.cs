using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;
using System.Linq;
using Framework;

public class SpawnUnitAction
{
    public Tile Tile { get; private set; }

    public SpawnUnitAction(Tile tile)
    {
        Tile = tile;
    }
}

public class SpawnMechAction : SpawnUnitAction
{
    public MechData MechData { get; private set; }

    public SpawnMechAction(MechData mech, Tile tile) : base(tile)
    {
        MechData = mech;
    }
}

public class GameboardObjects
{
    public event Action<Unit> UnitAdded;
    public event Action<Unit> UnitRemoved;

    private List<Unit> _units;

    private GameboardWorldHelper _gameboardTileMap;

    public GameboardObjects(GameboardWorldHelper gameboardTileMap)
    {
        _gameboardTileMap = gameboardTileMap;

        _units = new List<Unit>();
    }

    public void Spawn(Tile targetTile, MechData mechData)
    {
        Spawn<Mech>(targetTile, (mech) => mech.Initialize(mechData, _gameboardTileMap));
    }

    private void Spawn<T>(Tile targetTile, Action<T> onSpawn) where T : Unit
    {
        Assert.IsNotNull(_gameboardTileMap);
        Assert.IsNotNull(targetTile);

        if (targetTile.Blocked)
        {
            DebugEx.LogWarning<Gameboard>("Cannot place a unit at occupied tile '{0}'", targetTile.transform.GetGridPosition());
            return;
        }

        ObjectEx.Instantiate<T>((unit) =>
        {
            onSpawn(unit);

            targetTile.SetOccupant(unit);

            RegisterUnit(unit);
        });
    }

    private void RegisterUnit(Unit unit)
    {
        Assert.IsFalse(_units.Contains(unit));

        _units.Add(unit);

        UnitAdded.InvokeSafe(unit);

        unit.Removed += OnUnitKilled;
    }

    private void OnUnitKilled(Unit unit)
    {
        Assert.IsTrue(_units.Contains(unit));

        unit.Removed -= OnUnitKilled;

        _units.Remove(unit);

        UnitRemoved.InvokeSafe(unit);
    }
}