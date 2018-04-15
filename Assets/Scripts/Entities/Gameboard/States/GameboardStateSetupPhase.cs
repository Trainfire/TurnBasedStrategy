using UnityEngine.Assertions;
using Framework;

public class GameboardStateSetupPhase : GameboardStateBase
{
    public override GameboardStateID StateID { get { return GameboardStateID.Setup; } }

    private bool AllMechsSpawned { get { return _gameboardObjects.Mechs.Count == 3; } }

    private GameboardObjects _gameboardObjects;
    private IGameboardInputEvents _input;

    public GameboardStateSetupPhase(GameboardObjects gameboardObjects, IGameboardInputEvents input)
    {
        _gameboardObjects = gameboardObjects;
        _input = input;
    }

    protected override void OnEnter()
    {
        _gameboardObjects.UnitAdded += OnUnitAdded;
        _input.SpawnDefaultUnit += OnPlayerInputSpawnDefaultUnit;
        _input.Continue += OnPlayerInputContinue;
    }

    private void OnPlayerInputSpawnDefaultUnit(Tile targetTile)
    {
        Assert.IsNotNull(targetTile, "Cannot spawn a unit onto a null tile.");

        if (AllMechsSpawned)
        {
            DebugEx.LogWarning<GameboardStateSetupPhase>("Cannot spawn a mech as all mechs have been spawned.");
            return;
        }

        _gameboardObjects.Spawn(targetTile, Gameboard.DefaultMech);
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
            _gameboardObjects.UnitAdded -= OnUnitAdded;
            _input.SpawnDefaultUnit -= OnPlayerInputSpawnDefaultUnit;
            _input.Continue -= OnPlayerInputContinue;
            ExitState();
        }
    }
}