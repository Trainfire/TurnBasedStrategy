using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;
using System.Linq;
using Framework;

public class World : IWorldEvents
{
    public event Action<Unit> UnitAdded;
    public event Action<Unit> UnitRemoved;
    public event Action<Tile> TileAdded;

    public WorldParameters Parameters { get; private set; }
    public Helper Helper { get; private set; }
    public IWorldEvents Events { get { return this as IWorldEvents; } }
    public IReadOnlyDictionary<Vector2, Tile> Tiles { get { return _tiles; } }
    public IReadOnlyTwoWayDictionary<Tile, Unit> UnitsToTiles { get { return _unitHandler.UnitsToTiles; } }
    public IReadOnlyList<Unit> Units { get { return _units; } }
    public IReadOnlyList<Building> Buildings { get { return Units.Where(x => x.GetType() == typeof(Building)).Cast<Building>().ToList(); } }
    public IReadOnlyList<Mech> Mechs { get { return Units.Where(x => x.GetType() == typeof(Mech)).Cast<Mech>().ToList(); } }

    private Dictionary<Vector2, Tile> _tiles;
    private List<Unit> _units;
    private WorldUnitHandler _unitHandler;

    public World(WorldParameters parameters)
    {
        _tiles = new Dictionary<Vector2, Tile>();
        _units = new List<Unit>();
        _unitHandler = new WorldUnitHandler(this);

        Parameters = parameters;
        Helper = new Helper(this);

        new WorldGenerator().Generate(this);
    }

    public void SpawnUnit(Tile tile, MechData mechData)
    {
        SpawnUnit<Mech>(tile, new GameObject().AddComponent<Mech>(), (mech) => mech.Initialize(mechData, Helper));
    }

    public void SpawnUnit(Tile tile, Building prototype)
    {
        SpawnUnit<Building>(tile, prototype, (building) => building.Initialize(Helper));
    }

    private T SpawnUnit<T>(Tile tile, T prototype, Action<T> onSpawn) where T : Unit
    {
        var unit = GameObject.Instantiate<T>(prototype);

        _units.Add(unit);

        onSpawn(unit);

        UnitAdded?.Invoke(unit);

        unit.MoveTo(tile);

        return unit;
    }

    public Tile SpawnTile(WorldParameters worldParameters, int column, int row)
    {
        var position = new Vector2(column, row);

        var instance = GameObject.Instantiate(worldParameters.Data.Prefabs.Tile, worldParameters.Root);
        instance.name = string.Format("Tile {0}/{1}", column, row);
        instance.transform.SetGridPosition(position);

        instance.Initialize(Helper);
        //instance.OccupantChanged += AssignUnitToTile;
        //instance.OccupantLeft += RemoveUnitFromTile;

        _tiles.Add(position, instance);

        TileAdded?.Invoke(instance);

        return instance;
    }
}