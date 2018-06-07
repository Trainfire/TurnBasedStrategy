using Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class StateEnemyThinkPhase : StateBase
{
    public override StateID StateID { get { return StateID.EnemyThink; } }

    private class UnitTaskParameters
    {
        public Unit Actor { get; private set; }
        public Tile TargetPosition { get; set; }
        public Unit Target { get; set; }

        public UnitTaskParameters(Unit actor, Unit target)
        {
            Actor = actor;
            Target = target;
        }
    }

    private Queue<UnitTaskParameters> _tasks = new Queue<UnitTaskParameters>();

    protected override void OnEnter()
    {
        base.OnEnter();

        DebugEx.Log<StateEnemyMovePhase>("Start enemy think phase.");

        foreach (var enemy in Gameboard.World.Enemies.ToList())
        {
            var rnd = UnityEngine.Random.Range(0, Gameboard.World.Mechs.Count() - 1);

            _tasks.Enqueue(new UnitTaskParameters(enemy, Gameboard.World.Mechs[rnd]));
        }

        MoveNext();
    }

    private void MoveNext()
    {
        if (_tasks.Count == 0)
        {
            ExitState();
        }
        else
        {
            Assert.IsFalse(_tasks.Count == 0);

            var task = _tasks.Dequeue();

            DebugEx.Log<StateEnemyThinkPhase>("Executing task... {0} remaining...", _tasks.Count);

            var adjacentTiles = new List<Tile>();
            foreach (var tileResult in Gameboard.World.Helper.GetTilesInRadius(task.Target.Tile, 1))
            {
                if (!tileResult.Tile.Blocked)
                    adjacentTiles.Add(tileResult.Tile);
            }

            if (adjacentTiles.Count == 0)
            {
                DebugEx.LogWarning<StateEnemyThinkPhase>("Failed to find any free tiles surrounding the target '{0}'", task.Target.name);
                MoveNext();
            }

            var rnd = UnityEngine.Random.Range(0, adjacentTiles.Count);
            task.TargetPosition = adjacentTiles[rnd];

            ExecuteTask(task);
        }
    }

    private void ExecuteTask(UnitTaskParameters task)
    {
        task.Actor.MoveTo(task.TargetPosition);
        MoveNext();
        //var animator = new GameObject("EnemyThinkAnimator").AddComponent<UnitMoveAnimator>();
        //animator.Animate(Gameboard.World.Helper, task.Actor, task.TargetPosition, () =>
        //{
        //    task.Actor.MoveTo(task.TargetPosition);
        //    Destroy(animator.gameObject);
        //    //StartCoroutine(PostExecuteTask());
        //    MoveNext();
        //});
    }

    private IEnumerator PostExecuteTask()
    {
        yield return new WaitForSeconds(2f);
        MoveNext();
    }

    protected override void OnExit()
    {
        base.OnExit();
        Assert.IsTrue(_tasks.Count == 0);
    }
}