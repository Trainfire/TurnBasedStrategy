using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections.Generic;
using System;

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

public class Gameboard : MonoBehaviour
{
    public GameObject Prefab;
    public int GridSize;

    private Dictionary<Vector2, Tile> myGridTileMap;
    private Dictionary<Tile, int> myTraversalMap;
    private int myTraversalIterationCost;

    private void Start()
    {
        myGridTileMap = new Dictionary<Vector2, Tile>();
        myTraversalMap = new Dictionary<Tile, int>();

        for (int row = 0; row < GridSize; row++)
        {
            for (int column = 0; column < GridSize; column++)
            {
                var position = new Vector3(column, 0f, row);

                var gridTileInstance = GameObject.Instantiate(Prefab, transform);
                gridTileInstance.name = string.Format("Tile {0}/{1}", column, row);
                gridTileInstance.transform.position = position;

                var worldGridTile = gridTileInstance.GetComponent<Tile>();

                Assert.IsNotNull(worldGridTile, "Missing WorldGridTile component from specified prefab.");

                myGridTileMap.Add(position.TransformToGridspace(), worldGridTile);
            }
        }
    }

    public List<TileResult> GetTiles(Tile origin, GameboardDirection direction, int length, bool filterOccupiedTiles = false)
    {
        var vector = GridHelper.GetVectorFromDirection(direction);

        length = Mathf.Min(GridSize, length);

        var hitTiles = new List<TileResult>();

        // Start one tile away from origin.
        for (int i = 1; i < length + 1; i++)
        {
            var vectorFromDirection = GridHelper.GetVectorFromDirection(direction);
            var iteratorPosition = origin.transform.position.TransformToGridspace() + vectorFromDirection * i;

            if (myGridTileMap.ContainsKey(iteratorPosition))
            {
                var hitTile = myGridTileMap[iteratorPosition];

                if (filterOccupiedTiles && !hitTile.Occupied || !filterOccupiedTiles)
                    hitTiles.Add(new TileResult(hitTile, i));
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
        myTraversalMap.Clear();

        distance = Mathf.Clamp(distance, 1, GridSize);

        myTraversalIterationCost = 0;

        var queue = new Queue<TileResult>();
        queue.Enqueue(new TileResult(originTile, 0));

        while (queue.Count > 0)
        {
            myTraversalIterationCost++;

            var current = queue.Dequeue();
            if (current.Tile == null || current.Tile.Occupied || current.Distance > distance || current.Distance > GridSize || myTraversalMap.ContainsKey(current.Tile))
                continue;

            var direction = (current.Tile.Position - originTile.Position).normalized;

            if (direction.y != -1f) queue.Enqueue(new TileResult(GetTileInDirection(current.Tile, GameboardDirection.North), current.Distance + 1));
            if (direction.x != -1f) queue.Enqueue(new TileResult(GetTileInDirection(current.Tile, GameboardDirection.East), current.Distance + 1));
            if (direction.y != 1f) queue.Enqueue(new TileResult(GetTileInDirection(current.Tile, GameboardDirection.South), current.Distance + 1));
            if (direction.x != 1f) queue.Enqueue(new TileResult(GetTileInDirection(current.Tile, GameboardDirection.West), current.Distance + 1));

            if (current.Tile.Position != originTile.Position)
                myTraversalMap.Add(current.Tile, current.Distance);
        }

        var results = new List<TileResult>();
        foreach (var kvp in myTraversalMap)
        {
            if (kvp.Key.transform.position.TransformToGridspace() != Vector2.zero)
                results.Add(new TileResult(kvp.Key, kvp.Value));
        }

        return results;
    }

    Tile GetTileInDirection(Tile origin, GameboardDirection direction)
    {
        return GetTile(origin.Position + GridHelper.GetVectorFromDirection(direction));
    }

    public Tile GetTile(Vector2 position) { return myGridTileMap.ContainsKey(position) ? myGridTileMap[position] : null; }

    public void GetTile(Vector2 position, Action<Tile> onGet)
    {
        var tile = GetTile(position);
        if (tile != null)
            onGet(tile);
    }
}
