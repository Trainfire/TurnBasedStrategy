﻿using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Framework;

public enum GameboardStateID
{
    Invalid,
    Setup,
    PlayerMove,
    EnemyMove,
    GameOver,
}

public class GameboardStateFlags
{
    public bool CanContinue { get; set; }
    public bool CanControlUnits { get; set; }
    public bool CanSpawnUnits { get; set; }
    public bool CanUndo { get; set; }

    public void Clear()
    {
        CanContinue = false;
        CanControlUnits = false;
        CanSpawnUnits = false;
        CanUndo = false;
    }

    public ReadOnlyGameboardStateFlags AsReadOnly()
    {
        return new ReadOnlyGameboardStateFlags(this);
    }
}

public struct ReadOnlyGameboardStateFlags
{
    public readonly bool CanContinue;
    public readonly bool CanControlUnits;
    public readonly bool CanSpawnUnits;
    public readonly bool CanUndo;

    public ReadOnlyGameboardStateFlags(GameboardStateFlags validPlayerActions)
    {
        CanContinue = validPlayerActions.CanContinue;
        CanControlUnits = validPlayerActions.CanControlUnits;
        CanSpawnUnits = validPlayerActions.CanSpawnUnits;
        CanUndo = validPlayerActions.CanUndo;
    }
}


public class GameboardState
{
    public const int MaxTurns = 5; // TODO: Define via data.

    public event Action GameWon;
    public event Action GameOver;

    public Tile CurrentSelection { get; private set; }

    public GameboardStateID Current { get; private set; }
    public ReadOnlyGameboardStateFlags Flags { get { return _states[Current].Flags.AsReadOnly(); } }
    public int TurnCount { get; private set; }

    private Dictionary<GameboardStateID, GameboardStateBase> _states;

    private GameboardObjects _gameboardObjects;

    public GameboardState(Gameboard gameboard)
    {
        _gameboardObjects = gameboard.Objects;

        _states = new Dictionary<GameboardStateID, GameboardStateBase>();

        gameboard.InputEvents.Select += OnPlayerInputSelect;

        Register(new GameboardStateSetupPhase(gameboard.Objects, gameboard.InputEvents));
        Register(new GameboardStatePlayerMovePhase(this, gameboard));
        Register(new GameboardStateGameOver());

        foreach (var unit in gameboard.Objects.Units.ToList())
        {
            if (unit.GetType() == typeof(Building))
                unit.Health.Changed += OnBuildingHealthChanged;
        }

        Current = GameboardStateID.Setup;

        _states[Current].Enter();
    }

    private void Register(GameboardStateBase gameboardState)
    {
        Assert.IsFalse(_states.ContainsKey(gameboardState.StateID));

        _states.Add(gameboardState.StateID, gameboardState);

        gameboardState.Exited += MoveNext;
    }

    private void MoveNext(GameboardStateID previousStateID)
    {
        if (TurnCount == MaxTurns)
        {
            GameWon.InvokeSafe();
            return;
        }

        // TODO: Add logic for moving to other states.
        if (previousStateID == GameboardStateID.Setup)
            Current = GameboardStateID.PlayerMove;

        TurnCount++;

        Assert.IsTrue(_states.ContainsKey(Current), "No handler found for state.");

        _states[Current].Enter();
    }

    private void OnPlayerInputSelect(Tile selection)
    {
        if (selection == null)
            return;

        CurrentSelection = selection;

        DebugEx.Log<GameboardState>("Selected tile: " + CurrentSelection.name);
    }

    private void OnBuildingHealthChanged(HealthChangeEvent healthChangeEvent)
    {
        if (_gameboardObjects.Buildings.All(x => x.Health.Current == 0))
        {
            Current = GameboardStateID.GameOver;
            _states[Current].Enter();
            GameOver.InvokeSafe();
        }
    }
}

public abstract class GameboardStateBase
{
    public event Action<GameboardStateID> Exited;

    public abstract GameboardStateID StateID { get; }

    public virtual bool CanExitState { get { return true; } }
    public virtual GameboardStateFlags Flags { get; private set; }

    public void Enter()
    {
        Flags = new GameboardStateFlags();

        Debug.LogFormat("Enter state: [{0}]", StateID);
        OnEnter();
    }

    protected virtual void OnEnter() { }

    protected void ExitState()
    {
        Debug.LogFormat("Exit state: [{0}]", StateID);
        Flags.Clear();
        Exited.InvokeSafe(StateID);
    }
}

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

public class GameboardStatePlayerMovePhase : GameboardStateBase
{
    public override GameboardStateID StateID { get { return GameboardStateID.PlayerMove; } }

    private struct MoveUndoRecord
    {
        public Unit Unit { get; private set; }
        public Tile PreviousTile { get; private set; }

        public MoveUndoRecord(Unit unit, Tile previousTile)
        {
            Unit = unit;
            PreviousTile = previousTile;
        }

        public void Undo()
        {
            Unit.MoveTo(PreviousTile);
        }
    }

    private enum PlayerAction
    {
        Unassigned,
        Move,
        PrimaryAttack,
        SecondaryAttack,
    }

    private Stack<MoveUndoRecord> _moveUndoRecords;
    private PlayerAction _currentAction;
    private Tile _previousSelectedTile;
    private Mech _selectedMech;

    private GameboardState _state;
    private Gameboard _gameboard;

    public GameboardStatePlayerMovePhase(GameboardState state, Gameboard gameboard)
    {
        _state = state;
        _gameboard = gameboard;

        _moveUndoRecords = new Stack<MoveUndoRecord>();
    }

    protected override void OnEnter()
    {
        _moveUndoRecords.Clear();

        _gameboard.InputEvents.Undo += OnPlayerInputUndo;
        _gameboard.InputEvents.Continue += OnPlayerInputContinue;
        _gameboard.InputEvents.Select += OnPlayerInputSelect;
        _gameboard.InputEvents.SetCurrentActionToAttack += OnPlayerSetCurrentActionToAttack;
        _gameboard.InputEvents.SetCurrentActionToMove += OnPlayerSetCurrentActionToMove;
        _gameboard.InputEvents.CommitCurrentAction += OnPlayerCommitCurrentAction;
        _gameboard.InputEvents.PreviewAction += OnPlayerPreviewAction;

        Flags.CanControlUnits = true;
    }

    private void OnPlayerInputSelect(Tile targetTile)
    {
        if (targetTile != _previousSelectedTile)
        {
            _gameboard.Visualizer.Clear();
            _currentAction = PlayerAction.Unassigned;
        }

        _selectedMech = _state?.CurrentSelection?.Occupant as Mech;

        _previousSelectedTile = targetTile;
    }

    private void OnPlayerSetCurrentActionToMove()
    {
        if (_selectedMech == null)
            return;

        _currentAction = PlayerAction.Move;

        _gameboard.Visualizer.ShowReachablePositions(_selectedMech);
    }

    private void OnPlayerSetCurrentActionToAttack()
    {
        if (_selectedMech == null)
            return;

        if (_selectedMech.PrimaryWeapon == null || _selectedMech.PrimaryWeapon.WeaponData == null)
        {
            DebugEx.LogWarning<GameboardStatePlayerMovePhase>("Cannot attack using the primary weapon as the selected mech has no valid primary weapon and/or missing data.");
            return;
        }

        _currentAction = PlayerAction.PrimaryAttack;

        _gameboard.Visualizer.ShowTargetableTiles(_selectedMech, _selectedMech.PrimaryWeapon.WeaponData);
    }

    private void OnPlayerCommitCurrentAction(Tile targetTile)
    {
        if (targetTile == null || _selectedMech == null)
            return;

        switch (_currentAction)
        {
            case PlayerAction.Move:
                MoveUnit(_selectedMech, targetTile);
                break;
            case PlayerAction.PrimaryAttack:
                _selectedMech.PrimaryWeapon?.Use(targetTile);
                _moveUndoRecords.Clear();
                _gameboard.Objects.CommitStateAfterAttack();
                break;
            default: break;
        }

        _gameboard.Visualizer.Clear();

        UpdateFlags();
    }

    private void OnPlayerInputContinue()
    {
        _gameboard.InputEvents.Undo -= OnPlayerInputUndo;
        _gameboard.InputEvents.Continue -= OnPlayerInputContinue;
        _gameboard.InputEvents.Select -= OnPlayerInputSelect;
        _gameboard.InputEvents.SetCurrentActionToAttack -= OnPlayerSetCurrentActionToAttack;
        _gameboard.InputEvents.SetCurrentActionToMove -= OnPlayerSetCurrentActionToMove;
        _gameboard.InputEvents.CommitCurrentAction -= OnPlayerCommitCurrentAction;
        _gameboard.InputEvents.PreviewAction -= OnPlayerPreviewAction;

        Assert.IsTrue(_moveUndoRecords.Count == 0, "The move undo stack isn't empty when it should be.");

        ExitState();
    }

    private void OnPlayerInputUndo()
    {
        if (_moveUndoRecords.Count == 0)
            return;

        DebugEx.Log<GameboardStatePlayerMovePhase>("Undo");

        var moveUndoRecord = _moveUndoRecords.Pop();
        moveUndoRecord.Undo();

        _gameboard.Objects.RestoreStateBeforeMove();

        UpdateFlags();
    }

    /// <summary>
    /// DEBUG.
    /// </summary>
    /// <param name="previewTile"></param>
    private void OnPlayerPreviewAction(Tile previewTile)
    {
        if (_selectedMech == null)
        {
            DebugEx.LogWarning<GameboardStatePlayerMovePhase>("No mech selected.");
            return;
        }

        if (_currentAction == PlayerAction.Unassigned)
        {
            DebugEx.Log<GameboardStatePlayerMovePhase>("Nothing to preview.");
            return;
        }

        var effectPreview = new EffectPreview();

        if (_currentAction == PlayerAction.PrimaryAttack)
        {
            var mechTile = _gameboard.Helper.GetTile(_selectedMech);

            if (mechTile == null)
                return;

            if (_selectedMech.PrimaryWeapon == null || _selectedMech.PrimaryWeapon.WeaponData == null)
                return;

            var spawnEffectParameters = new SpawnEffectParameters(mechTile, previewTile);

            effectPreview = Effect.GetPreview(_selectedMech.PrimaryWeapon.WeaponData.EffectPrototype, _gameboard.Helper, spawnEffectParameters);
            
        }

        if (_currentAction == PlayerAction.Move)
            effectPreview = previewTile.Hazards.GetEffectPreview(HazardEffectTrigger.OnEnter);

        PrintPreview(effectPreview);
    }

    private void PrintPreview(EffectPreview effectPreview)
    {
        foreach (var tileHealthChange in effectPreview.HealthChanges)
        {
            DebugEx.Log<GameboardStatePlayerMovePhase>("Health Changes: {0} changes by {1}", tileHealthChange.Key.name, tileHealthChange.Value);
        }

        foreach (var tilePush in effectPreview.Pushes)
        {
            DebugEx.Log<GameboardStatePlayerMovePhase>("Push on tile {0} in direction {1}", tilePush.Key.name, tilePush.Value);
        }

        foreach (var tileCollisions in effectPreview.Collisions)
        {
            DebugEx.Log<GameboardStatePlayerMovePhase>("Collision on tile {0}", tileCollisions.name);
        }
    }

    private void MoveUnit(Mech mech, Tile targetTile)
    {
        if (!_gameboard.Helper.CanReachTile(mech.transform.GetGridPosition(), targetTile.transform.GetGridPosition(), mech.MovementRange))
            return;

        _gameboard.Objects.SaveStateBeforeMove();

        var moveRecord = new MoveUndoRecord(mech, _state.CurrentSelection);
        _moveUndoRecords.Push(moveRecord);

        mech.MoveTo(targetTile);

        UpdateFlags();
    }

    private void UpdateFlags()
    {
        Flags.CanUndo = _moveUndoRecords.Count != 0;
    }
}

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