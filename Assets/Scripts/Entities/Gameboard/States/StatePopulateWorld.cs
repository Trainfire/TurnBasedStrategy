using Framework;
using UnityEngine;
using System.Collections.Generic;

public class StatePopulateWorld : StateBase
{
    public override StateID StateID { get { return StateID.PopulateWorld; } }

    private TimedQueue<SpawnPoint> _spawnPoints;

    protected override void OnEnter()
    {
        base.OnEnter();

        DebugEx.Log<StatePopulateWorld>("Start enemy spawn.");

        Gameboard.World.Populate();

        if (Gameboard.World.Enemies.Count == 0)
        {
            if (_spawnPoints == null)
            {
                _spawnPoints = TimedQueue<SpawnPoint>.Create(transform);
                _spawnPoints.PreDelay = 0.25f;
                _spawnPoints.PostDelay = 0.25f;
                _spawnPoints.OnDequeue += (spawnPoint) => spawnPoint.Spawn();
                _spawnPoints.OnEmpty += ExitState;
            }

            _spawnPoints.Clear();
            _spawnPoints.Enqueue(Gameboard.World.SpawnPoints);
            _spawnPoints.Start();
        }
        else
        {
            ExitState();
        }
    }
}