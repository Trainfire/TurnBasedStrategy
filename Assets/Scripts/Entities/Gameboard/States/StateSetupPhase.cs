using UnityEngine.Assertions;
using Framework;

public class StateSetupPhase : StateBase
{
    public override StateID StateID { get { return StateID.Setup; } }

    private bool AllMechsSpawned { get { return Gameboard.Entities.Mechs.Count == 3; } }

    public StateSetupPhase(Gameboard gameboard, StateEventsController stateEventsController) : base(gameboard, stateEventsController) { }

    protected override void OnEnter()
    {
        Gameboard.Entities.UnitAdded += OnUnitAdded;
        Gameboard.InputEvents.SpawnDefaultUnit += OnPlayerInputSpawnDefaultUnit;
        Gameboard.InputEvents.Continue += OnPlayerInputContinue;
    }

    private void OnPlayerInputSpawnDefaultUnit(Tile targetTile)
    {
        Assert.IsNotNull(targetTile, "Cannot spawn a unit onto a null tile.");

        if (AllMechsSpawned)
        {
            DebugEx.LogWarning<StateSetupPhase>("Cannot spawn a mech as all mechs have been spawned.");
            return;
        }

        Gameboard.Entities.Spawn(targetTile, Gameboard.Data.Prefabs.DefaultMech);
    }

    private void OnUnitAdded(Unit unit)
    {
        if (!AllMechsSpawned)
            return;

        Flags.CanContinue = AllMechsSpawned;
        Flags.CanSpawnUnits = false;
    }

    private void OnPlayerInputContinue()
    {
        if (Flags.CanContinue)
        {
            Gameboard.Entities.UnitAdded -= OnUnitAdded;
            Gameboard.InputEvents.SpawnDefaultUnit -= OnPlayerInputSpawnDefaultUnit;
            Gameboard.InputEvents.Continue -= OnPlayerInputContinue;
            ExitState();
        }
    }
}