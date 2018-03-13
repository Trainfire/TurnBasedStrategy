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
