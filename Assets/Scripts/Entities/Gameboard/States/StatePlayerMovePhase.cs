using UnityEngine.Assertions;
using System.Collections.Generic;
using Framework;

public class StatePlayerMovePhase : StateBase
{
    public override StateID StateID { get { return StateID.PlayerMove; } }

    private StateUndoManager _undoManager = new StateUndoManager();
    private UnitActionHandler _unitActionHandler;
    private Mech _selectedMech;

    protected override void OnInitialize(Gameboard gameboard, StateEventsController gameboardEvents)
    {
        _unitActionHandler = gameObject.AddComponent<UnitActionHandler>();
        _unitActionHandler.Initialize(gameboardEvents);
    }

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

        Flags.CanContinue = true;
        Flags.CanControlUnits = true;
    }

    protected override void OnExit()
    {
        Gameboard.InputEvents.Undo -= OnPlayerInputUndo;
        Gameboard.InputEvents.Continue -= OnPlayerInputContinue;
        Gameboard.InputEvents.Select -= OnPlayerInputSelect;
        Gameboard.InputEvents.SetCurrentActionToAttack -= OnPlayerSetCurrentActionToAttack;
        Gameboard.InputEvents.SetCurrentActionToMove -= OnPlayerSetCurrentActionToMove;
        Gameboard.InputEvents.CommitCurrentAction -= OnPlayerCommitCurrentAction;
        Gameboard.InputEvents.HoveredTileChanged -= OnPlayerHoveredTileChanged;
    }

    private void OnPlayerInputSelect(Tile targetTile)
    {
        if (_unitActionHandler.Action != UnitAction.Unassigned)
        {
            Events.ClearPreview();
            Events.SetActionCancelled(new StateActionCancelledEventArgs());
        }

        _selectedMech = targetTile?.Occupant as Mech;
    }

    private void OnPlayerSetCurrentActionToMove()
    {
        if (_selectedMech == null)
            return;

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

        Events.ClearPreview();
        Events.SetActionToAttack(_selectedMech);
        Events.ShowPreview(Gameboard.World.Helper.GetTargetableTiles(_selectedMech, _selectedMech.PrimaryWeapon.WeaponData));
    }

    private void OnPlayerCommitCurrentAction(Tile targetTile)
    {
        if (targetTile == null || _selectedMech == null || !_unitActionHandler.Handler.IsValid(Gameboard.World.Helper, _selectedMech, targetTile))
            return;

        if (_unitActionHandler.Action == UnitAction.Move)
        {
            _undoManager.SavePosition(_selectedMech);
            SaveState();
        }

        Flags.CanControlUnits = false;
        Flags.CanContinue = false;
        Flags.CanUndo = false;

        _unitActionHandler.Handler.Execute(Gameboard.World.Helper, _selectedMech, targetTile, OnActionExecutionComplete);

        Events.ClearPreview();
        Events.SetActionCommitted(new StateActionCommittedEventArgs());
    }

    private void OnActionExecutionComplete(UnitActionExecutionCompletedResult unitActionExecutionResult)
    {
        if (unitActionExecutionResult.Action == UnitAction.PrimaryAttack || unitActionExecutionResult.Action == UnitAction.SecondaryAttack)
        {
            _undoManager.Clear();
            CommitState();
        }

        Flags.CanControlUnits = true;
        Flags.CanContinue = true;
        Flags.CanUndo = _undoManager.CanUndo;
    }

    private void OnPlayerInputContinue()
    {
        if (Flags.CanContinue)
            ExitState();
    }

    private void OnPlayerInputUndo()
    {
        if (!_undoManager.CanUndo)
            return;

        _undoManager.UndoLastMove();
        RestoreState();

        Flags.CanUndo = _undoManager.CanUndo;
    }

    private void OnPlayerHoveredTileChanged(Tile hoveredTile)
    {
        if (_unitActionHandler.Action == UnitAction.Unassigned)
            return;

        var effectPreview = new EffectPreview();

        if (hoveredTile != null)
        {
            if (_unitActionHandler.Action == UnitAction.PrimaryAttack)
            {
                var mechTile = Gameboard.World.Helper.GetTile(_selectedMech);
                if (mechTile == null || _selectedMech.PrimaryWeapon == null || _selectedMech.PrimaryWeapon.WeaponData == null)
                    return;

                var spawnEffectParameters = new SpawnEffectParameters(mechTile, hoveredTile);
                effectPreview = Effect.GetPreview(_selectedMech.PrimaryWeapon.WeaponData.EffectPrototype, Gameboard.World.Helper, spawnEffectParameters);
            }
            else if (_unitActionHandler.Action == UnitAction.Move)
            {
                effectPreview = hoveredTile.Hazards.GetEffectPreviewOnEnter(_selectedMech);
            }
        }

        Events.SetHoveredTile(hoveredTile);
        Events.ShowEffectPreview(effectPreview);
    }
}