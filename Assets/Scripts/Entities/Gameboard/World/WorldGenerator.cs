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
                var tile = world.SpawnTile(world.Parameters, column, row);

                // TEMP: Just add buildings to the last row.
                //if (row == world.Parameters.Data.GridSize - 1 && world.Parameters.Data.Prefabs.DefaultBuilding != null)
                //    world.SpawnUnit(tile, world.Parameters.Data.Prefabs.DefaultBuilding);
            }
        }
    }
}