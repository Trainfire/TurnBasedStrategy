using UnityEngine.Assertions;
using System.Collections.Generic;
using Framework;

public class StatePlayerMovePhase : StateBase
{
    public override StateID StateID { get { return StateID.PlayerMove; } }

    private enum PlayerAction
    {
        Unassigned,
        Move,
        PrimaryAttack,
        SecondaryAttack,
    }

    private StateUndoManager _undoManager = new StateUndoManager();
    private CachedValue<PlayerAction> _playerAction = new CachedValue<PlayerAction>();
    private Mech _selectedMech;

    public StatePlayerMovePhase(Gameboard gameboard, StateEventsController gameboardEvents) : base(gameboard, gameboardEvents) { }

    protected override void OnEnter()
    {
        _undoManager.Clear();

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

        _selectedMech = targetTile?.Occupant as Mech;
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
                _undoManager.Clear();
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

        ExitState();
    }

    private void OnPlayerInputUndo()
    {
        if (!_undoManager.CanUndo)
            return;

        DebugEx.Log<StatePlayerMovePhase>("Undo");

        _undoManager.UndoLastMove();

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
        if (!Gameboard.World.Helper.CanReachTile(_selectedMech.Tile.transform.GetGridPosition(), targetTile.transform.GetGridPosition(), mech.MovementRange))
            return;

        SaveState();

        _undoManager.SavePosition(_selectedMech);

        mech.MoveTo(targetTile);

        UpdateFlags();
    }

    private void UpdateFlags()
    {
        Flags.CanUndo = _undoManager.CanUndo;
    }
}