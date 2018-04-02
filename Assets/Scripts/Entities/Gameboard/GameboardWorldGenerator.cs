using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using Framework;

public class GameboardWorld
{
    public IReadOnlyDictionary<Vector2, Tile> Tiles { get; private set; }
    public int GridSize { get; private set; }

    public GameboardWorld(Dictionary<Vector2, Tile> tiles, int gridSize)
    {
        Tiles = tiles;
        GridSize = gridSize;
    }
}

public class GameboardWorldGenerator
{
    private Transform _root;
    private Tile _prototype;

    public GameboardWorldGenerator(Transform root, Tile prototype)
    {
        _root = root;
        _prototype = prototype;
    }

    public GameboardWorld Generate(int gridSize)
    {
        Assert.IsNotNull(_root);
        Assert.IsNotNull(_prototype);

        var tileMap = new Dictionary<Vector2, Tile>();

        for (int row = 0; row < gridSize; row++)
        {
            for (int column = 0; column < gridSize; column++)
            {
                var position = new Vector3(column, 0f, row);

                var gridTileInstance = GameObject.Instantiate(_prototype, _root);
                gridTileInstance.name = string.Format("Tile {0}/{1}", column, row);
                gridTileInstance.transform.position = position;

                var worldGridTile = gridTileInstance.gameObject.GetComponentAssert<Tile>();
                if (worldGridTile != null)
                    tileMap.Add(position.TransformToGridspace(), worldGridTile);
            }
        }

        return new GameboardWorld(tileMap, gridSize);
    }
}
