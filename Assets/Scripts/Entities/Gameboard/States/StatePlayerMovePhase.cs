using UnityEngine.Assertions;
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
        if (targetTile != _selectedTile.Previous)
        {
            _playerAction.Current = PlayerAction.Unassigned;
            Events.ClearPreview();
            Events.SetActionCancelled(new StateActionCancelledEventArgs());
        }

        if (targetTile.Occupant != null && (targetTile.Occupant as Mech) != null)
            _selectedMech = targetTile.Occupant as Mech;

        _selectedTile.Current = targetTile;
    }

    private void OnPlayerSetCurrentActionToMove()
    {
        if (_selectedMech == null)
            return;

        _playerAction.Current = PlayerAction.Move;

        var reachableTiles = Gameboard.Helper.GetReachableTiles(_selectedMech.transform.GetGridPosition(), _selectedMech.MovementRange);

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
        Events.ShowPreview(Gameboard.Helper.GetTargetableTiles(_selectedMech, _selectedMech.PrimaryWeapon.WeaponData));
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
                Gameboard.Entities.CommitStateAfterAttack();
                break;
            default: break;
        }

        Events.SetActionCommitted(new StateActionCommittedEventArgs());
        Events.ClearPreview();
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

        Gameboard.Entities.RestoreStateBeforeMove();

        UpdateFlags();
    }

    private void OnPlayerHoveredTileChanged(Tile hoveredTile)
    {
        var effectPreview = new EffectPreview();

        if (hoveredTile != null)
        {
            if (_playerAction.Current == PlayerAction.PrimaryAttack)
            {
                var mechTile = Gameboard.Helper.GetTile(_selectedMech);
                if (mechTile == null || _selectedMech.PrimaryWeapon == null || _selectedMech.PrimaryWeapon.WeaponData == null)
                    return;

                var spawnEffectParameters = new SpawnEffectParameters(mechTile, hoveredTile);
                effectPreview = Effect.GetPreview(_selectedMech.PrimaryWeapon.WeaponData.EffectPrototype, Gameboard.Helper, spawnEffectParameters);
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
        if (!Gameboard.Helper.CanReachTile(_selectedTile.Current.transform.GetGridPosition(), targetTile.transform.GetGridPosition(), mech.MovementRange))
            return;

        Gameboard.Entities.SaveStateBeforeMove();

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