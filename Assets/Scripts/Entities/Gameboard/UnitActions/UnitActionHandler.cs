using UnityEngine;
using System.Collections;

public enum UnitAction
{
    Unassigned,
    Move,
    PrimaryAttack,
    SecondaryAttack,
    Repair,
}

public class UnitActionHandler : MonoBehaviour
{
    private UnitAction _action;
    public UnitAction Action
    {
        get { return _action; }
        private set
        {
            _action = value;

            switch (_action)
            {
                case UnitAction.Move: _handler = new UnitMoveAction(); break;
                case UnitAction.PrimaryAttack: _handler = new UnitPrimaryAttackAction(); break;
                case UnitAction.Repair: _handler = new UnitRepairAction(); break;
                default: _handler = new UnitUnassignedAction(); break;
            }
        }
    }

    private IUnitAction _handler;
    public IUnitAction Handler { get { return _handler; } }

    private IStateEvents _stateEvents;

    public void Initialize(IStateEvents stateEvents)
    {
        Action = UnitAction.Unassigned;

        _stateEvents = stateEvents;
        _stateEvents.ActionSetToAttack += OnActionSetToAttack;
        _stateEvents.ActionCancelled += OnActionCancelled;
        _stateEvents.ActionSetToMove += OnActionSetToMove;
        _stateEvents.ActionSetToRepair += OnActionSetToRepair;
        _stateEvents.PostActionCommitted += OnPostActionCommitted;
    }

    private void OnActionSetToAttack(Mech obj) => Action = UnitAction.PrimaryAttack;
    private void OnActionSetToMove(StateActionSetToMoveEventArgs obj) => Action = UnitAction.Move;
    private void OnActionSetToRepair(Mech obj) => Action = UnitAction.Repair;
    private void OnActionCancelled(StateActionCancelledEventArgs obj) => Action = UnitAction.Unassigned;
    private void OnPostActionCommitted(StateActionCommittedEventArgs obj) => Action = UnitAction.Unassigned;

    private void OnDestroy()
    {
        _stateEvents.ActionSetToAttack -= OnActionSetToAttack;
        _stateEvents.ActionCancelled -= OnActionCancelled;
        _stateEvents.ActionSetToMove -= OnActionSetToMove;
        _stateEvents.ActionSetToRepair -= OnActionSetToRepair;
        _stateEvents.ActionCommitted -= OnPostActionCommitted;
    }
}
