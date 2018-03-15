using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections.Generic;

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

    private void Start()
    {
        myGridTileMap = new Dictionary<Vector2, Tile>();

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

    public List<TileResult> GetValidMovementTiles(Tile originTile, int distance)
    {
        var results = new List<TileResult>();
        var origin = originTile.transform.position.TransformToGridspace();

        distance = Mathf.Min(GridSize * 2, distance);

        for (int x = 0; x < distance + 1; x++)
        {
            for (int y = 0; y < distance + 1 - x; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                var rightUp = GetTile(origin + new Vector2(x, y));
                var rightDown = GetTile(origin + new Vector2(x, -y));

                var leftUp = GetTile(origin + new Vector2(-x, y));
                var leftDown = GetTile(origin + new Vector2(-x, -y));

                if (rightUp != null) results.Add(new TileResult(rightUp, x + y));
                if (rightDown != null) results.Add(new TileResult(rightDown, x + y));
                if (leftUp != null) results.Add(new TileResult(leftUp, x + y));
                if (leftDown != null) results.Add(new TileResult(leftDown, x + y));
            }
        }

        return results;
    }

    public Tile GetTile(Vector2 position)
    {
        return myGridTileMap.ContainsKey(position) ? myGridTileMap[position] : null;
    }
}
