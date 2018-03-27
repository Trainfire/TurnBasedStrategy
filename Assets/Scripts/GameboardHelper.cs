﻿using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;
using System.Linq;

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

public class PushbackResult
{
    public Unit Unit { get; private set; }
    public int Damage { get; private set; }

    public PushbackResult(Unit unit, int damage)
    {
        Unit = unit;
        Damage = damage;
    }
}

public class GameboardHelper
{
    public const int GridSize = 8;

    private Gameboard _gameBoard;
    private Dictionary<Tile, int> _traversalMap;

    public GameboardHelper(Gameboard gameboard)
    {
        Assert.IsNotNull(gameboard);
        _gameBoard = gameboard;
        _traversalMap = new Dictionary<Tile, int>();
    }

    public List<TileResult> GetTiles(Tile origin, WorldDirection direction, int offset, int length, bool filterOccupiedTiles = false)
    {
        length = Mathf.Min(_gameBoard.GridSize, length);

        var hitTiles = new List<TileResult>();

        // Start one tile away from origin.
        for (int i = offset + 1; i < offset + 1 + length; i++)
        {
            var vectorFromDirection = GridHelper.DirectionToVector(direction);

            var nextTile = GetTile(origin.transform.GetGridPosition() + vectorFromDirection * i);
            if (nextTile)
            {
                if (filterOccupiedTiles && !nextTile.Occupied || !filterOccupiedTiles)
                    hitTiles.Add(new TileResult(nextTile, i));
            }
        }

        return hitTiles;
    }

    public List<TileResult> GetTilesFromAllDirections(Tile origin, int offset, int length, bool filterOccupiedTiles = false)
    {
        var hitTiles = new List<TileResult>();

        foreach (var direction in GridHelper.AllDirections)
        {
            var offsetTile = GetTile(origin.transform.GetGridPosition() + GridHelper.DirectionToVector(direction) * offset);
            hitTiles.AddRange(GetTiles(offsetTile, direction, 0, length, filterOccupiedTiles));
        }

        return hitTiles;
    }

    public List<TileResult> GetTilesInRadius(Tile originTile, int distance)
    {
        var results = new List<TileResult>();

        for (int x = 0; x < distance + 1; x++)
        {
            for (int y = 0; y < distance - x + 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                GetTile(originTile.transform.GetGridPosition() + new Vector2(x, y), (tile) => results.Add(new TileResult(tile, x + y)));
                GetTile(originTile.transform.GetGridPosition() + new Vector2(x, -y), (tile) => results.Add(new TileResult(tile, x + y)));
                GetTile(originTile.transform.GetGridPosition() + new Vector2(-x, y), (tile) => results.Add(new TileResult(tile, x + y)));
                GetTile(originTile.transform.GetGridPosition() + new Vector2(-x, -y), (tile) => results.Add(new TileResult(tile, x + y)));
            }
        }

        return results;
    }

    public List<TileResult> GetReachableTiles(Tile originTile, int distance)
    {
        return GetReachableTiles(originTile.transform.GetGridPosition(), distance);
    }

    public List<TileResult> GetReachableTiles(Unit unit)
    {
        return GetReachableTiles(unit.transform.GetGridPosition(), unit.MovementRange);
    }

    public List<TileResult> GetReachableTiles(Vector2 gridPosition, int distance)
    {
        _traversalMap.Clear();

        distance = Mathf.Clamp(distance, 1, _gameBoard.GridSize);

        var queue = new Queue<TileResult>();
        queue.Enqueue(new TileResult(GetTile(gridPosition), 0));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current.Tile == null || current.Tile.Occupied && current.Tile.transform.GetGridPosition() != gridPosition || current.Distance > distance || current.Distance > _gameBoard.GridSize || _traversalMap.ContainsKey(current.Tile))
                continue;

            var direction = (current.Tile.transform.GetGridPosition() - gridPosition).normalized;

            if (direction.y != -1f) queue.Enqueue(new TileResult(GetTileInDirection(current.Tile, WorldDirection.North), current.Distance + 1));
            if (direction.x != -1f) queue.Enqueue(new TileResult(GetTileInDirection(current.Tile, WorldDirection.East), current.Distance + 1));
            if (direction.y != 1f) queue.Enqueue(new TileResult(GetTileInDirection(current.Tile, WorldDirection.South), current.Distance + 1));
            if (direction.x != 1f) queue.Enqueue(new TileResult(GetTileInDirection(current.Tile, WorldDirection.West), current.Distance + 1));

            if (current.Tile.transform.GetGridPosition() != gridPosition)
                _traversalMap.Add(current.Tile, current.Distance);
        }

        var results = new List<TileResult>();
        foreach (var kvp in _traversalMap)
        {
            if (kvp.Key.transform.GetGridPosition() != Vector2.zero)
                results.Add(new TileResult(kvp.Key, kvp.Value));
        }

        return results;
    }

    public List<TileResult> GetTargetableTiles(Unit unit, WeaponData weaponData)
    {
        var offset = Mathf.Max(weaponData.MinRange, 0);
        var length = weaponData.MaxRange == -1 ? _gameBoard.GridSize : weaponData.MaxRange;
        return GetTilesFromAllDirections(GetTile(unit), offset, length);
    }

    public List<PushbackResult> GetPushbackResults(Unit source, WorldDirection direction)
    {
        var results = new List<PushbackResult>();

        var nextTile = GetTile(source);

        while (nextTile != null && nextTile.Occupied)
        {
            results.Add(new PushbackResult(nextTile.Occupant, 1));

            nextTile = GetTileInDirection(nextTile, direction);
        }

        return results;
    }

    public bool CanReachTile(Vector2 origin, Vector2 target, int distance)
    {
        return GetReachableTiles(origin, distance).Any(x => x.Tile.transform.GetGridPosition() == target);
    }

    public bool CanReachTile(Vector2 origin, WorldDirection direction, int distance = 0)
    {
        return GetReachableTiles(origin, distance).Count > 0;
    }

    public bool CanAttackTile(Unit unit, Tile target, WeaponData weaponData)
    {
        return GetTargetableTiles(unit, weaponData).Any(x => x.Tile == target);
    }

    public Tile GetTileInDirection(Tile origin, WorldDirection direction)
    {
        return GetTile(origin.transform.GetGridPosition() + GridHelper.DirectionToVector(direction));
    }

    public Tile GetTileInDirection(Unit origin, WorldDirection direction)
    {
        return GetTile(origin.transform.GetGridPosition() + GridHelper.DirectionToVector(direction));
    }

    public Tile GetTile(Vector2 position)
    {
        return _gameBoard.TileMap.ContainsKey(position) ? _gameBoard.TileMap[position] : null;
    }

    public void GetTile(Vector2 position, Action<Tile> onGet)
    {
        var tile = GetTile(position);
        if (tile != null)
            onGet(tile);
    }

    public Tile GetTile(Unit unit)
    {
        return GetTile(unit.transform.GetGridPosition());
    }

    public static bool OutOfBounds(Vector2 position)
    {
        return position.x < 0f || position.x > GridSize || position.y < 0f || position.y > GridSize;
    }
}