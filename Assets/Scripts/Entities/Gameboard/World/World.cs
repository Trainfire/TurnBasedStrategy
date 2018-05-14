using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;
using System.Linq;
using Random = UnityEngine.Random;

public class World : IWorldEvents
{
    public event Action<Unit> UnitAdded;
    public event Action<Unit> UnitRemoved;
    public event Action<Tile> TileAdded;
    public event Action<SpawnPoint> SpawnPointAdded;

    public WorldParameters Parameters { get; private set; }
    public Helper Helper { get; private set; }
    public IWorldEvents Events { get { return this as IWorldEvents; } }
    public IReadOnlyDictionary<Vector2, Tile> Tiles { get { return _tiles; } }
    public IReadOnlyTwoWayDictionary<Tile, Unit> UnitsToTiles { get { return _unitHandler.UnitsToTiles; } }
    public IReadOnlyList<Unit> Units { get { return _units; } }
    public IReadOnlyList<Building> Buildings { get { return Units.Where(x => x.GetType() == typeof(Building)).Cast<Building>().ToList(); } }
    public IReadOnlyList<Mech> Mechs { get { return Units.Where(x => x.GetType() == typeof(Mech)).Cast<Mech>().ToList(); } }
    public IReadOnlyList<Enemy> Enemies { get { return Units.Where(x => x.GetType() == typeof(Enemy)).Cast<Enemy>().ToList(); } }
    public IReadOnlyList<SpawnPoint> SpawnPoints { get { return _spawnPoints; } }

    private Dictionary<Vector2, Tile> _tiles = new Dictionary<Vector2, Tile>();
    private List<Unit> _units = new List<Unit>();
    private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
    private WorldUnitHandler _unitHandler;

    public World(WorldParameters parameters)
    {
        _unitHandler = new WorldUnitHandler(this);

        Parameters = parameters;
        Helper = new Helper(this);

        new WorldGenerator().Generate(this);
    }

    public void Populate()
    {
        Debug.Log("Populating...");

        const int maxEnemyCount = 1;

        int spawnCount = Mathf.Max(0, maxEnemyCount - Enemies.Count);
        int spawned = 0;

        while (spawned != spawnCount)
        {
            var randomX = Random.Range(0, Helper.GridSize);
            var randomY = Random.Range(0, Helper.GridSize);

            var tile = Helper.GetTile(new Vector2(randomX, randomY));

            if (tile != null && !tile.Blocked)
            {
                var spawnPoint = AddSpawnPoint(tile);

                spawnPoint.Spawned += (args) =>
                {
                    if (!args.Tile.Blocked)
                        SpawnUnit(args.Tile, Parameters.Data.Prefabs.DefaultEnemy);

                    _spawnPoints.Remove(args);
                };

                spawned++;
            }
        }
    }

    public void SpawnUnit(Tile tile, MechData mechData)
    {
        SpawnUnit<Mech>(tile, new GameObject().AddComponent<Mech>(), (mech) => mech.Initialize(mechData, Helper));
    }

    public void SpawnUnit(Tile tile, Building prototype)
    {
        SpawnUnit<Building>(tile, prototype, (building) => building.Initialize(Helper));
    }

    public void SpawnUnit(Tile tile, EnemyData enemyData)
    {
        Assert.IsNotNull(enemyData, "Cannot spawn an enemy with null data.");
        SpawnUnit<Enemy>(tile, new GameObject().AddComponent<Enemy>(), (enemy) => enemy.Initialize(enemyData, Helper));
    }

    private SpawnPoint AddSpawnPoint(Tile tile)
    {
        var spawnPoint = new GameObject("Spawn Point").AddComponent<SpawnPoint>();
        spawnPoint.Initialize(tile);

        _spawnPoints.Add(spawnPoint);

        SpawnPointAdded?.Invoke(spawnPoint);

        return spawnPoint;
    }

    private T SpawnUnit<T>(Tile tile, T prototype, Action<T> onSpawn) where T : Unit
    {
        var unit = prototype == null ? new GameObject().AddComponent<T>() : GameObject.Instantiate<T>(prototype);
        unit.Removed += OnUnitRemoved;

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

        _tiles.Add(position, instance);

        TileAdded?.Invoke(instance);

        return instance;
    }

    private void OnUnitRemoved(Unit unit)
    {
        UnitRemoved?.Invoke(unit);

        unit.Removed -= OnUnitRemoved;

        _units.Remove(unit);
    }
}