using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections.Generic;
using System;
using Framework;

public enum WorldDirection
{
    North,
    East,
    South,
    West,
}

public enum RelativeDirection
{
    Forward,
    Right,
    Back,
    Left,
}

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

    public SpawnMechAction(MechData mech, Tile tile) : base (tile)
    {
        MechData = mech;
    }
}

public class Gameboard : GameEntity
{
    public event Action<Unit> UnitAdded;
    public event Action<Unit> UnitRemoved;

    public int GridSize { get { return _gridSize; } }
    public IReadOnlyDictionary<Vector2, Tile> TileMap { get { return _tileMap; } }
    public GameboardHelper Helper { get; private set; }
    public GameboardVisualizer Visualizer { get; private set; }

    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _gridSize;

    private List<Unit> _units;
    private Dictionary<Vector2, Tile> _tileMap;
    private GameboardHelper _helper;

    private void Awake()
    {
        _units = new List<Unit>();
        _tileMap = new Dictionary<Vector2, Tile>();

        Helper = new GameboardHelper(this);
        Visualizer = gameObject.GetComponentAssert<GameboardVisualizer>();
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();

        for (int row = 0; row < _gridSize; row++)
        {
            for (int column = 0; column < _gridSize; column++)
            {
                var position = new Vector3(column, 0f, row);

                var gridTileInstance = GameObject.Instantiate(_prefab, transform);
                gridTileInstance.name = string.Format("Tile {0}/{1}", column, row);
                gridTileInstance.transform.position = position;

                var worldGridTile = gridTileInstance.GetComponentAssert<Tile>();

                if (worldGridTile != null)
                    _tileMap.Add(position.TransformToGridspace(), worldGridTile);
            }
        }
    }

    public void Spawn(Tile targetTile, MechData mechData)
    {
        Spawn<Mech>(targetTile, (mech) => mech.Initialize(mechData, Helper));
    }

    private void Spawn<T>(Tile targetTile, Action<T> onSpawn) where T : Unit
    {
        Assert.IsNotNull(targetTile);

        if (targetTile.Occupied)
        {
            DebugEx.LogWarning<Gameboard>("Cannot place a unit at occupied tile '{0}'", targetTile.transform.GetGridPosition());
            return;
        }

        ObjectEx.Instantiate<T>((unit) =>
        {
            RegisterUnit(unit);

            onSpawn(unit);

            targetTile.SetOccupant(unit);

            UnitAdded.InvokeSafe(unit);
        });
    }

    private void RegisterUnit(Unit unit)
    {
        Assert.IsFalse(_units.Contains(unit));

        _units.Add(unit);

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
