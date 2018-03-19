using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections.Generic;
using System;
using Framework;

public enum GameboardDirection
{
    North,
    East,
    South,
    West,
}

public class TileResult
{
    public Tile Tile { get; private set; }
    public int Distance { get; private set; }

    public TileResult(Tile tile, int distance)
    {
        Tile = tile;
        Distance = distance;
    }
}

public class Gameboard : GameEntity
{
    public event Action<Unit> UnitAdded;
    public event Action<Unit> UnitRemoved;

    public int GridSize { get { return _gridSize; } }
    public IReadOnlyDictionary<Vector2, Tile> TileMap { get { return _tileMap; } }

    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _gridSize;

    private List<Unit> _units;
    private Dictionary<Vector2, Tile> _tileMap;

    private void Awake()
    {
        _units = new List<Unit>();
        _tileMap = new Dictionary<Vector2, Tile>();
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

    public bool CanSpawn(UnitData unitData, Tile tile)
    {
        Assert.IsNotNull(unitData);
        Assert.IsNotNull(tile);

        if (tile.Occupied)
        {
            DebugEx.LogWarning<Gameboard>("Cannot place a unit at occupied tile '{0}'", tile.transform.position.TransformToGridspace());
            return false;
        }

        if (unitData.Prefab == null)
        {
            DebugEx.LogWarning<Gameboard>("Cannot place unit '{0}' as the specified prefab is null.", unitData.Name);
            return false;
        }

        return true;
    }

    public void SpawnUnit(UnitData unitData, Tile tile)
    {
        if (CanSpawn(unitData, tile))
        {
            var unitInstance = GameObject.Instantiate(unitData.Prefab);
            var unitComponent = unitInstance.gameObject.GetOrAddComponent<Unit>();
            unitComponent.Initialize(unitData);

            _units.Add(unitComponent);
            unitComponent.Died += OnUnitDied;

            tile.SetOccupant(unitComponent);

            UnitAdded.InvokeSafe(unitComponent);
        }
    }

    private void OnUnitDied(Unit unit)
    {
        var hasUnit = _units.Contains(unit);

        Assert.IsTrue(hasUnit);

        if (!hasUnit)
            return;

        unit.Died -= OnUnitDied;

        _units.Remove(unit);

        UnitRemoved.InvokeSafe(unit);
    }
}
