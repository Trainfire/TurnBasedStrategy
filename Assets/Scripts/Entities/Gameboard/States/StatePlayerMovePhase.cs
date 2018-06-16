using UnityEngine.Assertions;
using System.Collections.Generic;
using Framework;
using System.Linq;

public class StatePlayerMovePhase : StateBase
{
    private class UnitFlags
    {
        public bool CanAttack { get; private set; } = true;
        public bool CanMove { get; private set; } = true;

        private Unit _unit;
        private IStateEvents _stateEvents;

        public UnitFlags(Unit unit, IStateEvents stateEvents)
        {
            _unit = unit;
            _unit.Health.Changed += OnHealthChanged;
            _unit.Removed += OnUnitRemoved;

            _stateEvents = stateEvents;
            _stateEvents.ActionCommitted += OnActionCommitted;
            _stateEvents.Undo += OnUndo;
        }

        private void OnHealthChanged(HealthChangeEvent args)
        {
            if (args.Health.Current == 0)
            {
                CanAttack = CanAttack ? false : CanAttack;
                CanMove = CanMove ? false : CanMove;
            }
        }

        private void OnUnitRemoved(Unit unit)
        {
            unit.Removed -= OnUnitRemoved;
            unit.Health.Changed -= OnHealthChanged;
        }

        private void OnActionCommitted(StateActionCommittedEventArgs args)
        {
            if (args.Unit != _unit)
                return;

            if (args.Action == UnitAction.PrimaryAttack || args.Action == UnitAction.SecondaryAttack)
            {
                CanAttack = false;
                CanMove = false;
            }
            else if (args.Action == UnitAction.Move)
            {
                CanMove = false;
            }
        }

        private void OnUndo(StateUndoEventArgs args)
        {
            if (args.Unit != _unit)
                return;

            CanMove = true;
        }
    }

    public override StateID StateID { get { return StateID.PlayerMove; } }

    private StateUndoManager _undoManager = new StateUndoManager();
    private UnitActionHandler _unitActionHandler;
    private Mech _selectedMech;
    private Dictionary<Unit, UnitFlags> _unitFlags = new Dictionary<Unit, UnitFlags>();

    protected override void OnInitialize(Gameboard gameboard, StateEventsController gameboardEvents)
    {
        _unitActionHandler = gameObject.AddComponent<UnitActionHandler>();
        _unitActionHandler.Initialize(gameboardEvents);
    }

    protected override void OnEnter()
    {
        _undoManager.Clear();
        _unitFlags.Clear();

        Gameboard.InputEvents.Undo += OnPlayerInputUndo;
        Gameboard.InputEvents.Continue += OnPlayerInputContinue;
        Gameboard.InputEvents.Select += OnPlayerInputSelect;
        Gameboard.InputEvents.SetCurrentActionToAttack += OnPlayerSetCurrentActionToAttack;
        Gameboard.InputEvents.SetCurrentActionToMove += OnPlayerSetCurrentActionToMove;
        Gameboard.InputEvents.CommitCurrentAction += OnPlayerCommitCurrentAction;
        Gameboard.InputEvents.HoveredTileChanged += OnPlayerHoveredTileChanged;

        Gameboard.World.Mechs.ToList().ForEach(mech => _unitFlags.Add(mech, new UnitFlags(mech, Events)));
        Flags.CanContinue = true;
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

        UpdateFlags();
    }

    private void OnPlayerSetCurrentActionToMove()
    {
        if (_selectedMech == null || !_unitFlags[_selectedMech].CanMove)
            return;

        var reachableTiles = Gameboard.World.Helper.GetReachableTiles(_selectedMech.transform.GetGridPosition(), _selectedMech.MovementRange);

        Events.ClearPreview();
        Events.SetActionToMove(new StateActionSetToMoveEventArgs(_selectedMech, reachableTiles));
        Events.ShowPreview(reachableTiles);
    }

    private void OnPlayerSetCurrentActionToAttack()
    {
        if (_selectedMech == null || !_unitFlags[_selectedMech].CanAttack)
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

        Flags.CanContinue = false;
        Flags.CanUndo = false;
        UpdateFlags();

        Events.ClearPreview();
        Events.SetActionCommitted(new StateActionCommittedEventArgs(_selectedMech, _unitActionHandler.Action));

        _unitActionHandler.Handler.Execute(Gameboard.World.Helper, _selectedMech, targetTile, OnActionExecutionComplete);
    }

    private void OnActionExecutionComplete(UnitActionExecutionCompletedResult unitActionExecutionResult)
    {
        if (unitActionExecutionResult.Action == UnitAction.PrimaryAttack || unitActionExecutionResult.Action == UnitAction.SecondaryAttack)
        {
            _undoManager.Clear();
            CommitState();
        }

        Events.SetPostActionCommitted(new StateActionCommittedEventArgs(_selectedMech, _unitActionHandler.Action));

        Flags.CanContinue = true;

        UpdateFlags();
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

        var undoEventArgs = _undoManager.UndoLastMove();
        Events.TriggerUndo(undoEventArgs);

        RestoreState();
        UpdateFlags();
    }

    private void OnPlayerHoveredTileChanged(Tile hoveredTile)
    {
        var effectPreview = new EffectPreview();

        if (hoveredTile != null)
        {
            if (_unitActionHandler.Action == UnitAction.PrimaryAttack)
            {
                var spawnEffectParameters = new SpawnEffectParameters(_selectedMech.Tile, hoveredTile);
                effectPreview = Effect.GetWeaponPreview(_selectedMech.PrimaryWeapon.WeaponData, Gameboard.World.Helper, spawnEffectParameters);
            }
            else if (_unitActionHandler.Action == UnitAction.Move)
            {
                effectPreview = hoveredTile.Hazards.GetEffectPreviewOnEnter(_selectedMech);
            }
            else if (hoveredTile.Occupant != null)
            {
                var aiControllerComponent = hoveredTile.Occupant.GetComponent<AIControllerComponent>();
                if (aiControllerComponent != null && aiControllerComponent.Target != null)
                {
                    var weaponComponent = hoveredTile.Occupant.GetComponent<UnitWeaponComponent>();
                    if (weaponComponent != null)
                    {
                        var spawnEffectParameters = new SpawnEffectParameters(hoveredTile, aiControllerComponent.Target);
                        effectPreview = Effect.GetWeaponPreview(weaponComponent.WeaponData, Gameboard.World.Helper, spawnEffectParameters);
                    }
                }
            }
        }

        Events.SetHoveredTile(hoveredTile);
        Events.ShowEffectPreview(effectPreview);
    }

    private void UpdateFlags()
    {
        Flags.CanUndo = _undoManager.CanUndo;
        Flags.CanSelectedUnitAttack = _selectedMech != null ? _unitFlags[_selectedMech].CanAttack : false;
        Flags.CanSelectedUnitMove = _selectedMech != null ? _unitFlags[_selectedMech].CanMove : false;
    }
}