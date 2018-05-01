using Framework;

public class StateGameOver : StateBase
{
    public override StateID StateID { get { return StateID.GameOver; } }

    protected override void OnEnter()
    {
        base.OnEnter();

        Flags.CanContinue = false;
        Flags.CanControlUnits = false;

        DebugEx.Log<StateGameOver>("Game over, man.");
    }
}