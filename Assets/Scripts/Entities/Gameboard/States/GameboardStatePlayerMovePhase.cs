using UnityEngine.Assertions;
using System.Collections.Generic;
using Framework;

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
    private CachedValue<PlayerAction> _playerAction;
    private CachedValue<Tile> _selectedTile;
    private Mech _selectedMech;

    private GameboardState _state;
    private Gameboard _gameboard;

    public GameboardStatePlayerMovePhase(GameboardState state, Gameboard gameboard)
    {
        _state = state;
        _gameboard = gameboard;

        _moveUndoRecords = new Stack<MoveUndoRecord>();

        _playerAction = new CachedValue<PlayerAction>(PlayerAction.Unassigned);
        _selectedTile = new CachedValue<Tile>();
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
        _gameboard.InputEvents.HoveredTileChanged += OnPlayerHoveredTileChanged;

        Flags.CanControlUnits = true;
    }

    private void OnPlayerInputSelect(Tile targetTile)
    {
        if (targetTile != _selectedTile.Previous)
        {
            _gameboard.Visualizer.Clear();
            _playerAction.Current = PlayerAction.Unassigned;
        }

        _selectedMech = _state?.CurrentSelection?.Occupant as Mech;

        _selectedTile.Current = targetTile;
    }

    private void OnPlayerSetCurrentActionToMove()
    {
        if (_selectedMech == null)
            return;

        _playerAction.Current = PlayerAction.Move;

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

        _playerAction.Current = PlayerAction.PrimaryAttack;

        _gameboard.Visualizer.ShowTargetableTiles(_selectedMech, _selectedMech.PrimaryWeapon.WeaponData);
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
        _gameboard.InputEvents.HoveredTileChanged -= OnPlayerHoveredTileChanged;

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

    private void OnPlayerHoveredTileChanged(Tile hoveredTile)
    {
        // TOOD: Add OnEnterPreview and OnLeavePreview events.

        var effectPreview = new EffectPreview();

        if (hoveredTile != null)
        {
            if (_playerAction.Current == PlayerAction.PrimaryAttack)
            {
                var mechTile = _gameboard.Helper.GetTile(_selectedMech);
                if (mechTile == null || _selectedMech.PrimaryWeapon == null || _selectedMech.PrimaryWeapon.WeaponData == null)
                    return;

                var spawnEffectParameters = new SpawnEffectParameters(mechTile, hoveredTile);
                effectPreview = Effect.GetPreview(_selectedMech.PrimaryWeapon.WeaponData.EffectPrototype, _gameboard.Helper, spawnEffectParameters);
            }
            else if (_playerAction.Current == PlayerAction.Move)
            {
                effectPreview = hoveredTile.Hazards.GetEffectPreviewOnEnter(_selectedMech);
            }
        }

        PreviewEffect(effectPreview);
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