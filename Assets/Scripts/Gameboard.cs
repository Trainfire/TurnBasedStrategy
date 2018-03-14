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

public class ValidMovementTileResult
{
    public GameboardTile Tile { get; private set; }
    public int Distance { get; private set; }
    public float ActualDistance { get; private set; }

    public ValidMovementTileResult(GameboardTile tile, int distance, float actualDistance)
    {
        Tile = tile;
        Distance = distance;
        ActualDistance = actualDistance;
    }
}

public class Gameboard : MonoBehaviour
{
    public GameObject Prefab;
    public int GridSize;

    private Dictionary<Vector3, GameboardTile> myGridTileMap;

    private void Start()
    {
        myGridTileMap = new Dictionary<Vector3, GameboardTile>();

        for (int row = 0; row < GridSize; row++)
        {
            for (int column = 0; column < GridSize; column++)
            {
                var position = new Vector3(column, 0f, row);

                var gridTileInstance = GameObject.Instantiate(Prefab, transform);
                gridTileInstance.name = string.Format("Tile {0}/{1}", column, row);
                gridTileInstance.transform.position = position;

                var worldGridTile = gridTileInstance.GetComponent<GameboardTile>();

                Assert.IsNotNull(worldGridTile, "Missing WorldGridTile component from specified prefab.");

                myGridTileMap.Add(position, worldGridTile);
            }
        }
    }

    public List<GameboardTile> GetTiles(GameboardTile origin, GameboardDirection direction, int length, bool filterOccupiedTiles = false)
    {
        var vector = GetVectorFromDirection(direction);

        length = Mathf.Min(GridSize, length);

        var hitTiles = new List<GameboardTile>();

        // Start one tile away from origin.
        for (int i = 1; i < length + 1; i++)
        {
            var iteratorPosition = origin.transform.position + GetVectorFromDirection(direction) * i;

            if (myGridTileMap.ContainsKey(iteratorPosition))
            {
                var hitTile = myGridTileMap[iteratorPosition];

                if (filterOccupiedTiles && !hitTile.Occupied || !filterOccupiedTiles)
                    hitTiles.Add(hitTile);
            }
        }

        return hitTiles;
    }

    public List<GameboardTile> GetTilesFromAllDirections(GameboardTile origin, int length, bool filterOccupiedTiles = false)
    {
        var directions = new List<GameboardDirection>();
        directions.Add(GameboardDirection.North);
        directions.Add(GameboardDirection.East);
        directions.Add(GameboardDirection.South);
        directions.Add(GameboardDirection.West);

        var hitTiles = new List<GameboardTile>();

        foreach (var direction in directions)
        {
            hitTiles.AddRange(GetTiles(origin, direction, length, filterOccupiedTiles));
        }

        return hitTiles;
    }

    public List<ValidMovementTileResult> GetValidMovementTiles(GameboardTile origin, int radius)
    {
        var results = new List<ValidMovementTileResult>();

        //foreach (var tile in myGridTileMap.Values)
        //{
        //    var actualDistance = Vector3.Distance(tile.transform.position, origin.transform.position);
        //    var distance = Mathf.CeilToInt(actualDistance);

        //    //if (distance != 0 && (distance <= movement || Mathf.RoundToInt(actualDistance) == movement - 1))
        //        results.Add(new ValidMovementTileResult(tile, distance, actualDistance));
        //}

        for (int x = 0; x < radius; x++)
        {
            for (int y = 0; y < radius - x; y++)
            {
                var tileA = GetTile(new Vector2(x, y));
                var tileB = GetTile(new Vector2(x, -y));
                var tileC = GetTile(new Vector2(-x, y));
                var tileD = GetTile(new Vector2(-x, y));

                if (tileA != null)
                    results.Add(new ValidMovementTileResult(tileA, x + y, x));

                if (tileB != null)
                    results.Add(new ValidMovementTileResult(tileB, x + y, x));

                if (tileC != null)
                    results.Add(new ValidMovementTileResult(tileC, x + y, x));

                if (tileD != null)
                    results.Add(new ValidMovementTileResult(tileD, x + y, x));
            }
        }

        return results;
    }

    public GameboardTile GetTile(Vector2 position)
    {
        var gridPosition = ToGridPosition(position);
        return myGridTileMap.ContainsKey(gridPosition) ? myGridTileMap[gridPosition] : null;
    }

    Vector3 ToGridPosition(Vector2 position)
    {
        return new Vector3(position.x, 0f, position.y);
    }

    Vector3 GetVectorFromDirection(GameboardDirection direction)
    {
        switch (direction)
        {
            case GameboardDirection.North: return new Vector3(0f, 0f, 1f);
            case GameboardDirection.East: return new Vector3(1f, 0f, 0f);
            case GameboardDirection.South: return new Vector3(0f, 0f, -1f);
            case GameboardDirection.West: return new Vector3(-1f, 0f, 0f);
        }

        return Vector3.zero;
    }
}
