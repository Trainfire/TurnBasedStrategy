using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;
using System.Linq;
using Framework;

public class SpawnUnitAction
{
    public Tile Tile { get; private set; }

    public SpawnUnitAction(Tile tile)
    {
        Tile = tile;
    }
}

public class SpawnMechAction : SpawnUnitAction
{
    public MechData MechData { get; private set; }

    public SpawnMechAction(MechData mech, Tile tile) : base(tile)
    {
        MechData = mech;
    }
}

public class GameboardObjects
{
    public event Action<Unit> UnitAdded;
    public event Action<Unit> UnitRemoved;

    public IReadOnlyList<Unit> Units { get { return _units; } }
    public IReadOnlyList<Mech> Mechs { get { return _units.OfType<Mech>().ToList(); } }
    public IReadOnlyList<Building> Buildings { get { return _units.OfType<Building>().ToList(); } }

    private List<Unit> _units;
    private List<IStateHandler> _stateHandlers;

    private GameboardWorldHelper _worldHelper;

    public GameboardObjects(GameboardWorldHelper worldHelper, GameboardWorld world)
    {
        _worldHelper = worldHelper;

        _units = new List<Unit>();
        _stateHandlers = new List<IStateHandler>();

        world.Units.ToList().ForEach(unit =>
        {
            unit.Initialize(worldHelper);
            RegisterUnit(unit);
        });

        world.Tiles.ToList().ForEach(tileMap => RegisterStateHandler(tileMap.Value));
    }

    public void SaveStateBeforeMove() => _stateHandlers.ForEach(x => x.SaveStateBeforeMove());
    public void RestoreStateBeforeMove() => _stateHandlers.ForEach(x => x.RestoreStateBeforeMove());
    public void CommitStateAfterAttack() => _stateHandlers.ForEach(x => x.CommitStateAfterAttack());

    public void Spawn(Tile targetTile, MechData mechData)
    {
        Spawn<Mech>(targetTile, (mech) => mech.Initialize(mechData, _worldHelper));
    }

    private void Spawn<T>(Tile targetTile, Action<T> onSpawn) where T : Unit
    {
        Assert.IsNotNull(_worldHelper);
        Assert.IsNotNull(targetTile);

        if (targetTile.Blocked)
        {
            DebugEx.LogWarning<Gameboard>("Cannot place a unit at occupied tile '{0}'", targetTile.transform.GetGridPosition());
            return;
        }

        ObjectEx.Instantiate<T>((unit) =>
        {
            onSpawn(unit);

            targetTile.SetOccupant(unit);

            RegisterUnit(unit);
        });
    }

    private void RegisterUnit(Unit unit)
    {
        Assert.IsFalse(_units.Contains(unit));

        _units.Add(unit);

        UnitAdded.InvokeSafe(unit);

        unit.Removed += OnUnitKilled;

        RegisterStateHandler(unit.Health);
    }

    private void RegisterStateHandler(IStateHandler stateHandler)
    {
        Assert.IsFalse(_stateHandlers.Contains(stateHandler));

        _stateHandlers.Add(stateHandler);
    }

    private void OnUnitKilled(Unit unit)
    {
        Assert.IsTrue(_units.Contains(unit));

        unit.Removed -= OnUnitKilled;

        _units.Remove(unit);

        UnitRemoved.InvokeSafe(unit);
    }
}