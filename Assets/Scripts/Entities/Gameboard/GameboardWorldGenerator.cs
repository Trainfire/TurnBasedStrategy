using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using Framework;

public class GameboardWorld
{
    public IReadOnlyDictionary<Vector2, Tile> Tiles { get; private set; }
    public IReadOnlyList<Unit> Units { get; private set; }
    public int GridSize { get; private set; }

    public GameboardWorld(Dictionary<Vector2, Tile> tiles, List<Unit> units, int gridSize)
    {
        Tiles = tiles;
        Units = units;
        GridSize = gridSize;
    }
}

public class GameboardWorldParameters
{
    public Transform Root { get; private set; }
    public Tile TilePrototype { get; private set; }
    public List<Building> BuildingPrototypes { get; private set; }

    public GameboardWorldParameters(Transform root, Tile tilePrototype)
    {
        Root = root;
        TilePrototype = tilePrototype;
        BuildingPrototypes = new List<Building>();
    }

    public void Add(Building buildingPrototype)
    {
        BuildingPrototypes.Add(buildingPrototype);
    }
}

public class GameboardWorldGenerator
{
    private GameboardWorldParameters _gameboardWorldParameters;

    public GameboardWorldGenerator(GameboardWorldParameters gameboardWorldParameters)
    {
        _gameboardWorldParameters = gameboardWorldParameters;
    }

    public GameboardWorld Generate(int gridSize)
    {
        Assert.IsNotNull(_gameboardWorldParameters.Root);
        Assert.IsNotNull(_gameboardWorldParameters.TilePrototype);

        var tileMap = new Dictionary<Vector2, Tile>();
        var units = new List<Unit>();

        for (int row = 0; row < gridSize; row++)
        {
            for (int column = 0; column < gridSize; column++)
            {
                var position = new Vector3(column, 0f, row);

                var gridTileInstance = GameObject.Instantiate(_gameboardWorldParameters.TilePrototype, _gameboardWorldParameters.Root);
                gridTileInstance.name = string.Format("Tile {0}/{1}", column, row);
                gridTileInstance.transform.position = position;

                var worldGridTile = gridTileInstance.gameObject.GetComponentAssert<Tile>();
                if (worldGridTile != null)
                    tileMap.Add(position.TransformToGridspace(), worldGridTile);

                // TEMP: Just add buildings to the last row.
                if (row == gridSize - 1 && _gameboardWorldParameters.BuildingPrototypes.Count != 0)
                {
                    var building = GameObject.Instantiate(_gameboardWorldParameters.BuildingPrototypes[0], _gameboardWorldParameters.Root);
                    units.Add(building);

                    worldGridTile.SetOccupant(building);
                }
            }
        }

        return new GameboardWorld(tileMap, units, gridSize);
    }
}
