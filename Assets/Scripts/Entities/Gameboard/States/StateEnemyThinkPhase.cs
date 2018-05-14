using Framework;

public class StateEnemyThinkPhase : StateBase
{
    public override StateID StateID { get { return StateID.EnemyThink; } }

    protected override void OnEnter()
    {
        base.OnEnter();

        DebugEx.Log<StateEnemyMovePhase>("Start enemy think phase.");

        ExitState();
    }
}