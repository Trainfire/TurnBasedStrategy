using Framework;
using System.Collections.Generic;
using System.Linq;

public class StateTriggerSpawnPoints : StateBase
{
    public override StateID StateID { get { return StateID.TriggerSpawnPoints; } }

    private Queue<SpawnPoint> _spawnPoints = new Queue<SpawnPoint>();

    protected override void OnEnter()
    {
        base.OnEnter();

        DebugEx.Log<StateTriggerSpawnPoints>("Populating world.");

        _spawnPoints = new Queue<SpawnPoint>(Gameboard.World.SpawnPoints);

        MoveNext();
    }

    private void MoveNext()
    {
        if (_spawnPoints.Count != 0)
        {
            _spawnPoints.Dequeue().Spawn();
            MoveNext();
        }
        else
        {
            ExitState();
        }
    }
}