using Framework;
using UnityEngine;

public class StateEnemySpawnPhase : StateBase
{
    public override StateID StateID { get { return StateID.EnemySpawn; } }

    protected override void OnEnter()
    {
        base.OnEnter();

        DebugEx.Log<StateEnemyMovePhase>("Start enemy spawn.");

        const int maxEnemyCount = 3;
        int spawnCount = Mathf.Max(0, maxEnemyCount - Gameboard.World.Enemies.Count);
        int spawned = 0;
        while (spawned != spawnCount)
        {
            var randomX = Random.Range(0, Gameboard.World.Helper.GridSize);
            var randomY = Random.Range(0, Gameboard.World.Helper.GridSize);

            var tile = Gameboard.World.Helper.GetTile(new Vector2(randomX, randomY));

            if (tile != null && !tile.Blocked)
            {
                Gameboard.World.SpawnUnit(tile, Gameboard.Data.Prefabs.DefaultEnemy);
                spawned++;
            }
        }

        ExitState();
    }
}