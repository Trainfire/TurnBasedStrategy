﻿using Framework;
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

    private const float TimeBetweenMoves = 1f; // TODO: Move to Data.

    private Queue<AIControllerComponent> _aiControllers = new Queue<AIControllerComponent>();

    protected override void OnEnter()
    {
        base.OnEnter();

        DebugEx.Log<StateEnemyActPhase>("Start enemy think phase.");

        var targets = new List<Unit>();
        Gameboard.World.Mechs.ToList().ForEach(x => targets.Add(x));
        Gameboard.World.Buildings.ToList().ForEach(x => targets.Add(x));

        foreach (var enemy in Gameboard.World.Enemies.ToList())
        {
            var rnd = UnityEngine.Random.Range(0, targets.Count);

            var aiControllerComponent = enemy.GetComponent<AIControllerComponent>();
            if (aiControllerComponent != null)
                aiControllerComponent.Target = targets[rnd].Tile;

            _aiControllers.Enqueue(aiControllerComponent);
        }

        MoveNext();
    }

    private void MoveNext()
    {
        if (_aiControllers.Count == 0)
        {
            ExitState();
        }
        else
        {
            var aiController = _aiControllers.Peek();

            DebugEx.Log<StateEnemyThinkPhase>("Executing task... {0} remaining...", _aiControllers.Count);

            aiController.Move(() => StartCoroutine(PostMove()));
        }
    }

    private IEnumerator PostMove()
    {
        yield return new WaitForSeconds(TimeBetweenMoves);

        var aiController = _aiControllers.Dequeue();

        Events.ShowAttackTargetPreview(aiController.Target);
        
        yield return new WaitForSeconds(TimeBetweenMoves);

        MoveNext();
    }

    protected override void OnExit()
    {
        base.OnExit();
        Assert.IsTrue(_aiControllers.Count == 0);
    }
}