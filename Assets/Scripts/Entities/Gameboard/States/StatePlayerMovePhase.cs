﻿using UnityEngine.Assertions;
using System.Collections.Generic;
using Framework;

public class StatePlayerMovePhase : StateBase
{
    public override StateID StateID { get { return StateID.PlayerMove; } }

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
    private CachedValue<PlayerAction> _playerAction;
    private CachedValue<Tile> _selectedTile;
    private Mech _selectedMech;

    public StatePlayerMovePhase(Gameboard gameboard, StateEventsController gameboardEvents) : base(gameboard, gameboardEvents)
    {
        _moveUndoRecords = new Stack<MoveUndoRecord>();

        _playerAction = new CachedValue<PlayerAction>(PlayerAction.Unassigned);
        _selectedTile = new CachedValue<Tile>();
    }

    protected override void OnEnter()
    {
        _moveUndoRecords.Clear();

        Gameboard.InputEvents.Undo += OnPlayerInputUndo;
        Gameboard.InputEvents.Continue += OnPlayerInputContinue;
        Gameboard.InputEvents.Select += OnPlayerInputSelect;
        Gameboard.InputEvents.SetCurrentActionToAttack += OnPlayerSetCurrentActionToAttack;
        Gameboard.InputEvents.SetCurrentActionToMove += OnPlayerSetCurrentActionToMove;
        Gameboard.InputEvents.CommitCurrentAction += OnPlayerCommitCurrentAction;
        Gameboard.InputEvents.HoveredTileChanged += OnPlayerHoveredTileChanged;

        Flags.CanControlUnits = true;
    }

    private void OnPlayerInputSelect(Tile targetTile)
    {
        if (_playerAction.Current != PlayerAction.Unassigned)
        {
            _playerAction.Current = PlayerAction.Unassigned;
            Events.ClearPreview();
            Events.SetActionCancelled(new StateActionCancelledEventArgs());
        }

        var mech = targetTile?.Occupant as Mech;
        if (mech != null)
            _selectedMech = targetTile.Occupant as Mech;

        _selectedTile.Current = targetTile;
    }

    private void OnPlayerSetCurrentActionToMove()
    {
        if (_selectedMech == null)
            return;

        _playerAction.Current = PlayerAction.Move;

        var reachableTiles = Gameboard.World.Helper.GetReachableTiles(_selectedMech.transform.GetGridPosition(), _selectedMech.MovementRange);

        Events.ClearPreview();
        Events.SetActionToMove(new StateActionSetToMoveEventArgs(_selectedMech, reachableTiles));
        Events.ShowPreview(reachableTiles);
    }

    private void OnPlayerSetCurrentActionToAttack()
    {
        if (_selectedMech == null)
            return;

        if (_selectedMech.PrimaryWeapon == null || _selectedMech.PrimaryWeapon.WeaponData == null)
        {
            DebugEx.LogWarning<StatePlayerMovePhase>("Cannot attack using the primary weapon as the selected mech has no valid primary weapon and/or missing data.");
            return;
        }

        _playerAction.Current = PlayerAction.PrimaryAttack;

        Events.ClearPreview();
        Events.SetActionToAttack(_selectedMech);
        Events.ShowPreview(Gameboard.World.Helper.GetTargetableTiles(_selectedMech, _selectedMech.PrimaryWeapon.WeaponData));
    }

    private void OnPlayerCommitCurrentAction(Tile targetTile)
    {
        if (targetTile == null || _selectedMech == null)
            return;

        switch (_playerAction.Current)
        {
            case PlayerAction.Move:
                MoveUnit(_selectedMech, targetTile);
                break;
            case PlayerAction.PrimaryAttack:
                _selectedMech.PrimaryWeapon?.Use(targetTile);
                _moveUndoRecords.Clear();
                CommitState();
                break;
            default: break;
        }

        Events.ClearPreview();
        Events.SetActionCommitted(new StateActionCommittedEventArgs());

        _playerAction.Current = PlayerAction.Unassigned;

        UpdateFlags();
    }

    private void OnPlayerInputContinue()
    {
        Gameboard.InputEvents.Undo -= OnPlayerInputUndo;
        Gameboard.InputEvents.Continue -= OnPlayerInputContinue;
        Gameboard.InputEvents.Select -= OnPlayerInputSelect;
        Gameboard.InputEvents.SetCurrentActionToAttack -= OnPlayerSetCurrentActionToAttack;
        Gameboard.InputEvents.SetCurrentActionToMove -= OnPlayerSetCurrentActionToMove;
        Gameboard.InputEvents.CommitCurrentAction -= OnPlayerCommitCurrentAction;
        Gameboard.InputEvents.HoveredTileChanged -= OnPlayerHoveredTileChanged;

        Assert.IsTrue(_moveUndoRecords.Count == 0, "The move undo stack isn't empty when it should be.");

        ExitState();
    }

    private void OnPlayerInputUndo()
    {
        if (_moveUndoRecords.Count == 0)
            return;

        DebugEx.Log<StatePlayerMovePhase>("Undo");

        var moveUndoRecord = _moveUndoRecords.Pop();
        moveUndoRecord.Undo();

        RestoreState();

        UpdateFlags();
    }

    private void OnPlayerHoveredTileChanged(Tile hoveredTile)
    {
        if (_playerAction.Current == PlayerAction.Unassigned)
            return;

        var effectPreview = new EffectPreview();

        if (hoveredTile != null)
        {
            if (_playerAction.Current == PlayerAction.PrimaryAttack)
            {
                var mechTile = Gameboard.World.Helper.GetTile(_selectedMech);
                if (mechTile == null || _selectedMech.PrimaryWeapon == null || _selectedMech.PrimaryWeapon.WeaponData == null)
                    return;

                var spawnEffectParameters = new SpawnEffectParameters(mechTile, hoveredTile);
                effectPreview = Effect.GetPreview(_selectedMech.PrimaryWeapon.WeaponData.EffectPrototype, Gameboard.World.Helper, spawnEffectParameters);
            }
            else if (_playerAction.Current == PlayerAction.Move)
            {
                effectPreview = hoveredTile.Hazards.GetEffectPreviewOnEnter(_selectedMech);
            }
        }

        Events.SetHoveredTile(hoveredTile);
        Events.ShowEffectPreview(effectPreview);
    }

    private void MoveUnit(Mech mech, Tile targetTile)
    {
        if (!Gameboard.World.Helper.CanReachTile(_selectedTile.Current.transform.GetGridPosition(), targetTile.transform.GetGridPosition(), mech.MovementRange))
            return;

        SaveState();

        var moveRecord = new MoveUndoRecord(mech, _selectedTile.Current);
        _moveUndoRecords.Push(moveRecord);

        mech.MoveTo(targetTile);

        UpdateFlags();
    }

    private void UpdateFlags()
    {
        Flags.CanUndo = _moveUndoRecords.Count != 0;
    }
}