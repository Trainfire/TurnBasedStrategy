using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;
using System.Linq;

public class GameboardHelper
{
    private Gameboard _gameBoard;
    private Dictionary<Tile, int> _traversalMap;

    public GameboardHelper(Gameboard gameboard)
    {
        Assert.IsNotNull(gameboard);
        _gameBoard = gameboard;
        _traversalMap = new Dictionary<Tile, int>();
    }

    public List<TileResult> GetTiles(Tile origin, GameboardDirection direction, int length, bool filterOccupiedTiles = false)
    {
        length = Mathf.Min(_gameBoard.GridSize, length);

        var hitTiles = new List<TileResult>();

        // Start one tile away from origin.
        for (int i = 1; i < length + 1; i++)
        {
            var vectorFromDirection = GridHelper.GetVectorFromDirection(direction);
            //var iteratorPosition = origin.transform.position.TransformToGridspace() + vectorFromDirection * i;

            var nextTile = GetTile(origin.Position + vectorFromDirection * i);
            if (nextTile)
            {
                if (filterOccupiedTiles && !nextTile.Occupied || !filterOccupiedTiles)
                    hitTiles.Add(new TileResult(nextTile, i));
            }
        }

        return hitTiles;
    }

    public List<TileResult> GetTilesFromAllDirections(Tile origin, int length, bool filterOccupiedTiles = false)
    {
        var hitTiles = new List<TileResult>();

        foreach (var direction in GridHelper.AllDirections)
        {
            hitTiles.AddRange(GetTiles(origin, direction, length, filterOccupiedTiles));
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

                GetTile(originTile.Position + new Vector2(x, y), (tile) => results.Add(new TileResult(tile, x + y)));
                GetTile(originTile.Position + new Vector2(x, -y), (tile) => results.Add(new TileResult(tile, x + y)));
                GetTile(originTile.Position + new Vector2(-x, y), (tile) => results.Add(new TileResult(tile, x + y)));
                GetTile(originTile.Position + new Vector2(-x, -y), (tile) => results.Add(new TileResult(tile, x + y)));
            }
        }

        return results;
    }

    public List<TileResult> GetReachableTiles(Tile originTile, int distance)
    {
        return GetReachableTiles(originTile.Position, distance);
    }

    public List<TileResult> GetReachableTiles(Unit unit)
    {
        return GetReachableTiles(unit.transform.position.TransformToGridspace(), unit.MovementRange);
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
            if (current.Tile == null || current.Tile.Occupied && current.Tile.Position != gridPosition || current.Distance > distance || current.Distance > _gameBoard.GridSize || _traversalMap.ContainsKey(current.Tile))
                continue;

            var direction = (current.Tile.Position - gridPosition).normalized;

            if (direction.y != -1f) queue.Enqueue(new TileResult(GetTileInDirection(current.Tile, GameboardDirection.North), current.Distance + 1));
            if (direction.x != -1f) queue.Enqueue(new TileResult(GetTileInDirection(current.Tile, GameboardDirection.East), current.Distance + 1));
            if (direction.y != 1f) queue.Enqueue(new TileResult(GetTileInDirection(current.Tile, GameboardDirection.South), current.Distance + 1));
            if (direction.x != 1f) queue.Enqueue(new TileResult(GetTileInDirection(current.Tile, GameboardDirection.West), current.Distance + 1));

            if (current.Tile.Position != gridPosition)
                _traversalMap.Add(current.Tile, current.Distance);
        }

        var results = new List<TileResult>();
        foreach (var kvp in _traversalMap)
        {
            if (kvp.Key.transform.position.TransformToGridspace() != Vector2.zero)
                results.Add(new TileResult(kvp.Key, kvp.Value));
        }

        return results;
    }

    public bool CanReachTile(Vector2 origin, Vector2 target, int distance)
    {
        return GetReachableTiles(origin, distance).Any(x => x.Tile.Position == target);
    }

    public Tile GetTileInDirection(Tile origin, GameboardDirection direction)
    {
        return GetTile(origin.Position + GridHelper.GetVectorFromDirection(direction));
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
        return GetTile(unit.transform.position.TransformToGridspace());
    }
}