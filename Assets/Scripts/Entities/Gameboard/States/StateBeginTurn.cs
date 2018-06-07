using Framework;
using UnityEngine;

public class StateBeginTurn : StateBase
{
    public override StateID StateID { get { return StateID.BeginTurn; } }

    protected override void OnEnter()
    {
        base.OnEnter();

        DebugEx.Log<StateEnemyActPhase>("Start enemy spawn.");

        Gameboard.World.Populate();

        ExitState();
    }
}