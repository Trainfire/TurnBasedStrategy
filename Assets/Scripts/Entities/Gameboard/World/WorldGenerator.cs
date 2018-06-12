using UnityEngine.Assertions;

public class WorldGenerator
{
    public void Generate(World world)
    {
        Assert.IsNotNull(world.Parameters.Root);
        Assert.IsNotNull(world.Parameters.Data);

        for (int row = 0; row < world.Parameters.Data.GridSize; row++)
        {
            for (int column = 0; column < world.Parameters.Data.GridSize; column++)
            {
                world.SpawnTile(world.Parameters, column, row);
            }
        }

        const int minBuildings = 5;
        int spawnedBuildings = 0;
        int iterations = 0;

        while (spawnedBuildings != minBuildings && iterations < world.Helper.GridSize * world.Helper.GridSize)
        {
            var rndX = UnityEngine.Random.Range(0, world.Helper.GridSize);
            var rndY = UnityEngine.Random.Range(0, world.Helper.GridSize);

            var tile = world.Helper.GetTile(new UnityEngine.Vector2(rndX, rndY));
            if (!tile.Blocked)
            {
                spawnedBuildings++;
                world.SpawnUnit(tile, world.Parameters.Data.Prefabs.DefaultBuilding);
            }
        }
    }
}