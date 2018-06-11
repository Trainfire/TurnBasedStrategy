using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Framework;

public class StateEnemyActPhase : StateBase
{
    public override StateID StateID { get { return StateID.EnemyAct; } }

    private const float TimeBetweenActions = 1f;
    private Queue<AIControllerComponent> _aiControllers = new Queue<AIControllerComponent>();

    protected override void OnEnter()
    {
        base.OnEnter();

        DebugEx.Log<StateEnemyActPhase>("Start enemy move phase.");

        foreach (var enemy in Gameboard.World.Enemies)
        {
            var aiControllerComponent = enemy.GetComponent<AIControllerComponent>();
            if (aiControllerComponent != null)
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
            var aiController = _aiControllers.Dequeue();

            DebugEx.Log<StateEnemyThinkPhase>("Executing task... {0} remaining...", _aiControllers.Count);

            aiController.Act(() => StartCoroutine(PostAct()));
        }
    }

    private IEnumerator PostAct()
    {
        yield return new WaitForSeconds(TimeBetweenActions);

        MoveNext();
    }
}