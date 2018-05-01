using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

public class VisualizerMovePreviewer
{
    private IStateEvents _stateEvents;
    private StateActionSetToMoveEventArgs _moveEventArgs;

    public VisualizerMovePreviewer(IStateEvents stateEvents)
    {
        _stateEvents = stateEvents;
        _stateEvents.ActionCancelled += OnActionCancelled;
        _stateEvents.ActionSetToMove += OnActionSetToMove;
        _stateEvents.ActionCommitted += OnActionCommitted;
        _stateEvents.HoveredTileSet += OnHoveredTileSet;
        _stateEvents.PreviewDisabled += OnPreviewDisabled;
    }

    private void OnActionCancelled(StateActionCancelledEventArgs obj)
    {
        if (_moveEventArgs == null || _moveEventArgs.Mech == null)
            return;

        _moveEventArgs.Mech.transform.SetGridPosition(_moveEventArgs.StartPosition);
        _moveEventArgs = null;
    }

    private void OnActionSetToMove(StateActionSetToMoveEventArgs eventArgs)
    {
        _moveEventArgs = eventArgs;
    }

    private void OnActionCommitted(StateActionCommittedEventArgs eventArgs)
    {
        _moveEventArgs = null;
    }

    private void OnHoveredTileSet(Tile targetTile)
    {
        if (_moveEventArgs == null || _moveEventArgs.Mech == null || targetTile == null)
            return;

        var isTargetTileValid = _moveEventArgs.ReachableTiles.Any(result => result.Tile == targetTile);

        var position = isTargetTileValid ? targetTile.transform.GetGridPosition() : _moveEventArgs.StartPosition;

        _moveEventArgs.Mech.transform.SetGridPosition(position);
    }

    private void OnPreviewDisabled()
    {
        if (_moveEventArgs != null)
            _moveEventArgs.Mech.transform.SetGridPosition(_moveEventArgs.StartPosition);
    }
}