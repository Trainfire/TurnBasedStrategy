using Framework;

public class StateEnemyMovePhase : StateBase
{
    public override StateID StateID { get { return StateID.EnemyMove; } }

    protected override void OnEnter()
    {
        base.OnEnter();

        DebugEx.Log<StateEnemyMovePhase>("Start enemy phase.");

        ExitState();
    }
}