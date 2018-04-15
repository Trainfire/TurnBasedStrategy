using Framework;

public class GameboardStateGameOver : GameboardStateBase
{
    public override GameboardStateID StateID { get { return GameboardStateID.GameOver; } }

    protected override void OnEnter()
    {
        base.OnEnter();

        Flags.CanContinue = false;
        Flags.CanControlUnits = false;

        DebugEx.Log<GameboardStateGameOver>("Game over, man.");
    }
}