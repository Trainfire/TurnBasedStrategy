using Framework;
using UnityEngine;

public class StatePopulateWorld : StateBase
{
    public override StateID StateID { get { return StateID.PopulateWorld; } }

    protected override void OnEnter()
    {
        base.OnEnter();

        DebugEx.Log<StatePopulateWorld>("Start enemy spawn.");

        Gameboard.World.Populate();

        ExitState();
    }
}