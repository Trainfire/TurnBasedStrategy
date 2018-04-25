using UnityEngine.Assertions;
using UnityEditor;
using Framework;

public class WorldUnitHandler
{
    public IReadOnlyTwoWayDictionary<Tile, Unit> UnitsToTiles { get { return _unitsToTiles; } }

    private TwoWayDictionary<Tile, Unit> _unitsToTiles = new TwoWayDictionary<Tile, Unit>();

    public WorldUnitHandler(IWorldEvents worldEvents)
    {
        worldEvents.UnitAdded += OnUnitAdded;
    }

    private void OnUnitAdded(Unit unit)
    {
        unit.Moved += OnUnitMoved;
        unit.Removed += OnUnitRemoved;
    }

    private void OnUnitMoved(UnitMoveEvent args)
    {
        UnregisterUnit(args.Unit);
        AssignUnitToTile(args.TargetTile, args.Unit);
    }

    private void OnUnitRemoved(Unit unit)
    {
        UnregisterUnit(unit);
    }

    private void UnregisterTile(Tile tile)
    {
        if (_unitsToTiles.Contains(tile))
        {
            var unit = _unitsToTiles[tile];
            tile.Leave(unit);
            _unitsToTiles.Remove(unit, tile);
        }
    }

    private void UnregisterUnit(Unit unit)
    {
        if (_unitsToTiles.Contains(unit))
        {
            var tile = _unitsToTiles[unit];
            tile.Leave(unit);
            _unitsToTiles.Remove(unit, tile);
        }
    }

    private void AssignUnitToTile(Tile tile, Unit unit)
    {
        UnregisterTile(tile);

        Assert.IsFalse(_unitsToTiles.Contains(tile));
        Assert.IsFalse(_unitsToTiles.Contains(unit));

        _unitsToTiles.Add(tile, unit);

        tile.Enter(unit);

        unit.transform.SetGridPosition(tile.transform.GetGridPosition());

        DebugEx.Log<WorldUnitHandler>("Unit moved to tile " + tile.name);
    }
}