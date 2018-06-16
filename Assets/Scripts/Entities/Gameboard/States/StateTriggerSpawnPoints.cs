using UnityEngine;
using Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class StateTriggerSpawnPoints : StateBase
{
    public override StateID StateID { get { return StateID.TriggerSpawnPoints; } }

    private Queue<SpawnPoint> _spawnPoints = new Queue<SpawnPoint>();

    protected override void OnEnter()
    {
        base.OnEnter();

        DebugEx.Log<StateTriggerSpawnPoints>("Triggering spawn points.");

        _spawnPoints = new Queue<SpawnPoint>(Gameboard.World.SpawnPoints);

        MoveNext();
    }

    private void MoveNext()
    {
        if (_spawnPoints.Count != 0)
        {
            _spawnPoints.Dequeue().Spawn();
            StartCoroutine(PostSpawn());
        }
        else
        {
            ExitState();
        }
    }

    IEnumerator PostSpawn()
    {
        yield return new WaitForSeconds(1f);
        MoveNext();
    }
}