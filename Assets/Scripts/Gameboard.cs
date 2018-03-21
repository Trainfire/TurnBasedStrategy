﻿using UnityEngine;
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

public struct SpawnAction
{
    public UnitData Unit;
    public Tile Tile;

    public SpawnAction(UnitData unit, Tile tile)
    {
        Unit = unit;
        Tile = tile;
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

    public bool CanSpawn(SpawnAction spawnAction)
    {
        Assert.IsNotNull(spawnAction.Tile);
        Assert.IsNotNull(spawnAction.Unit);

        if (spawnAction.Tile.Occupied)
        {
            DebugEx.LogWarning<Gameboard>("Cannot place a unit at occupied tile '{0}'", spawnAction.Tile.transform.position.TransformToGridspace());
            return false;
        }

        if (spawnAction.Unit.Prefab == null)
        {
            DebugEx.LogWarning<Gameboard>("Cannot place unit '{0}' as the specified prefab is null.", spawnAction.Unit.Name);
            return false;
        }

        return true;
    }

    public void SpawnUnit(SpawnAction spawnAction)
    {
        if (CanSpawn(spawnAction))
        {
            var unitInstance = GameObject.Instantiate(spawnAction.Unit.Prefab);
            var unitComponent = unitInstance.gameObject.GetOrAddComponent<Unit>();
            unitComponent.Initialize(spawnAction.Unit, spawnAction.Tile, Helper);

            _units.Add(unitComponent);

            unitComponent.Died += OnUnitDied;

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

        Destroy(unit.gameObject);
    }
}
