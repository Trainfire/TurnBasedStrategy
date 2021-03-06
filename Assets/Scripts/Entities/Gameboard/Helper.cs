﻿using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;
using System.Linq;
using Framework;

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

public class Helper
{
    public enum TileFilterMode
    {
        None,
        /// <summary>Get all tiles until hitting a blocked tile.</summary>
        ContigiousTiles,
        /// <summary>Get all tiles until hitting a blocked tile. Includes the blocked tile.</summary>
        ContiguousTilesInclusive,
    }

    public int GridSize { get { return _world.Parameters.Data.GridSize; } }

    private World _world;

    public Helper(World world)
    {
        _world = world;
    }

    public List<TileResult> GetTiles(Tile origin, WorldDirection direction, int offset, int length, TileFilterMode filterMode = TileFilterMode.None)
    {
        Assert.IsNotNull(origin, "Origin tile is null.");

        length = Mathf.Min(GridSize, length);

        var hitTiles = new List<TileResult>();

        // Start one tile away from origin.
        for (int i = offset + 1; i < offset + 1 + length; i++)
        {
            var vectorFromDirection = GridHelper.DirectionToVector(direction);

            var nextTile = GetTile(origin.transform.GetGridPosition() + vectorFromDirection * i);
            if (nextTile)
            {
                if (filterMode != TileFilterMode.None && nextTile.Blocked)
                {
                    if (filterMode == TileFilterMode.ContiguousTilesInclusive)
                        hitTiles.Add(new TileResult(nextTile, i));

                    break;
                }

                hitTiles.Add(new TileResult(nextTile, i));
            }
        }

        return hitTiles;
    }

    public List<TileResult> GetTilesFromAllDirections(Tile origin, int offset, int length, TileFilterMode filterMode = TileFilterMode.None)
    {
        Assert.IsNotNull(origin);

        var hitTiles = new List<TileResult>();

        foreach (var direction in GridHelper.AllDirections)
        {
            var offsetTile = GetTile(origin.transform.GetGridPosition() + GridHelper.DirectionToVector(direction) * offset);
            if (offsetTile != null)
                hitTiles.AddRange(GetTiles(offsetTile, direction, 0, length, filterMode));
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

    public List<TileResult> GetReachableTiles(Vector2 gridPosition, int distance)
    {
        var traversalMap = new Dictionary<Tile, int>();

        distance = Mathf.Clamp(distance, 1, GridSize);

        var queue = new Queue<TileResult>();
        queue.Enqueue(new TileResult(GetTile(gridPosition), 0));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current.Tile == null || current.Tile.Blocked && current.Tile.transform.GetGridPosition() != gridPosition || current.Distance > distance || current.Distance > GridSize || traversalMap.ContainsKey(current.Tile))
                continue;

            var direction = (current.Tile.transform.GetGridPosition() - gridPosition).normalized;

            if (direction.y != -1f) queue.Enqueue(new TileResult(GetTileInDirection(current.Tile, WorldDirection.North), current.Distance + 1));
            if (direction.x != -1f) queue.Enqueue(new TileResult(GetTileInDirection(current.Tile, WorldDirection.East), current.Distance + 1));
            if (direction.y != 1f) queue.Enqueue(new TileResult(GetTileInDirection(current.Tile, WorldDirection.South), current.Distance + 1));
            if (direction.x != 1f) queue.Enqueue(new TileResult(GetTileInDirection(current.Tile, WorldDirection.West), current.Distance + 1));

            if (current.Tile.transform.GetGridPosition() != gridPosition)
                traversalMap.Add(current.Tile, current.Distance);
        }

        var results = new List<TileResult>();
        foreach (var kvp in traversalMap)
        {
            if (kvp.Key.transform.GetGridPosition() != Vector2.zero)
                results.Add(new TileResult(kvp.Key, kvp.Value));
        }

        return results;
    }

    public List<TileResult> GetPath(Tile start, Tile end)
    {
        if (start == end)
            return new List<TileResult>();

        var visitedTiles = new Dictionary<Tile, int>();
        visitedTiles.Add(start, 0);

        var queue = new Queue<TileResult>();
        queue.Enqueue(new TileResult(start, 0));

        // Explore map recursively and track how far the tiles are from the start.
        while (queue.Count != 0)
        {
            var current = queue.Dequeue();

            for (int i = 0; i < GridHelper.AllDirections.Count(); i++)
            {
                var candidateTile = GetTileInDirection(current.Tile, GridHelper.AllDirections.ToArray()[i]);

                if (candidateTile != null && !candidateTile.Blocked)
                {
                    var tileResult = new TileResult(candidateTile, current.Distance + 1);

                    if (visitedTiles.ContainsKey(candidateTile))
                    {
                        if (visitedTiles[candidateTile] > tileResult.Distance)
                            visitedTiles[candidateTile] = tileResult.Distance;
                    }
                    else
                    {
                        visitedTiles.Add(tileResult.Tile, tileResult.Distance);

                        if (candidateTile == end)
                            break;

                        queue.Enqueue(tileResult);
                    }
                }
            }
        }

        if (!visitedTiles.ContainsKey(end))
            return new List<TileResult>();

        // Group tiles by distance.
        var distancesToTilesMap = new Dictionary<int, List<Tile>>();
        foreach (var kvp in visitedTiles)
        {
            if (kvp.Value < visitedTiles[end])
            {
                if (!distancesToTilesMap.ContainsKey(kvp.Value))
                    distancesToTilesMap.Add(kvp.Value, new List<Tile>());

                distancesToTilesMap[kvp.Value].Add(kvp.Key);
            }
        }

        // Start from end tile and move towards the next closest tile.
        var path = new Stack<TileResult>();
        path.Push(new TileResult(end, distancesToTilesMap.Count));

        for (int i = distancesToTilesMap.Count - 1; i > 0; i--)
        {
            Tile closest = null;
            var lastDistance = -1f;

            foreach (var candidateTile in distancesToTilesMap[i])
            {
                var distance = Vector2.Distance(candidateTile.transform.GetGridPosition(), path.Peek().Tile.transform.GetGridPosition());

                if (distance < lastDistance || lastDistance == -1f)
                {
                    closest = candidateTile;
                    lastDistance = distance;
                }
            }

            Assert.IsNotNull(closest, "Failed to find a closest tile. You done goofed, bro.");

            path.Push(new TileResult(closest, i));
        }

        return path.ToList();
    }

    public List<TileResult> GetTargetableTiles(Unit unit, WeaponData weaponData)
    {
        return GetTargetableTiles(GetTile(unit), weaponData);
    }

    public List<TileResult> GetTargetableTiles(Tile source, WeaponData weaponData)
    {
        var offset = Mathf.Max(weaponData.MinRange, 0);
        var length = weaponData.MaxRange == -1 ? GridSize : weaponData.MaxRange;
        var filterMode = weaponData.WeaponType == WeaponType.Projectile ? TileFilterMode.ContiguousTilesInclusive : TileFilterMode.None;
        return GetTilesFromAllDirections(source, offset, length, filterMode);
    }

    public List<Tile> GetCollisions(Unit source, WorldDirection direction)
    {
        var results = new List<Tile>();

        var nextTile = GetTileInDirection(source, direction);

        while (nextTile != null && nextTile.Occupant != null)
        {
            results.Add(nextTile);
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
        return _world.Tiles.ContainsKey(position) ? _world.Tiles[position] : null;
    }

    public void GetTile(Vector2 position, Action<Tile> onGet)
    {
        var tile = GetTile(position);
        if (tile != null)
            onGet(tile);
    }

    public Tile GetTile(Unit unit)
    {
        return _world.UnitsToTiles.Contains(unit) ? _world.UnitsToTiles[unit] : null;
    }

    public Unit GetUnit(Tile tile)
    {
        return _world.UnitsToTiles.Contains(tile) ? _world.UnitsToTiles[tile] : null;
    }

    public static bool OutOfBounds(Vector2 position)
    {
        // TODO.
        return false;
        //return position.x < 0f || position.x > GridSize || position.y < 0f || position.y > GridSize;
    }
}