using UnityEngine;
using System.Collections.Generic;
using System;

public enum GameEndedResultState
{
    Unassigned,
    Loss,
    Win,
}

public struct GameEndedResult
{
    public GameEndedResultState GameEndedResultState { get; private set; }

    public GameEndedResult(GameEndedResultState gameEndedResultState)
    {
        GameEndedResultState = gameEndedResultState;
    }
}

public interface IStateEvents
{
    event Action<GameEndedResult> GameEnded;
    event Action<StateActionCancelledEventArgs> ActionCancelled;
    event Action<StateActionSetToMoveEventArgs> ActionSetToMove;
    event Action<Mech> ActionSetToAttack;
    event Action<StateActionCommittedEventArgs> ActionCommitted;
    event Action<Tile> HoveredTileSet;
    event Action<List<TileResult>> PreviewEnabled;
    event Action PreviewDisabled;
    event Action<EffectPreview> EffectPreviewShow;
}

public class StateActionSetToMoveEventArgs
{
    public Mech Mech { get; private set; }
    public Vector2 StartPosition { get; private set; }
    public List<TileResult> ReachableTiles { get; private set; }

    public StateActionSetToMoveEventArgs(Mech mech, List<TileResult> reachableTiles)
    {
        Mech = mech;
        StartPosition = mech.transform.GetGridPosition();
        ReachableTiles = reachableTiles;
    }
}

public class StateActionCommittedEventArgs
{
    public StateActionCommittedEventArgs() { }
}

public class StateActionCancelledEventArgs
{
    public StateActionCancelledEventArgs() { }
}

public class StateEventsController : MonoBehaviour, IStateEvents
{
    public event Action<GameEndedResult> GameEnded;
    public event Action<StateActionCancelledEventArgs> ActionCancelled;
    public event Action<StateActionSetToMoveEventArgs> ActionSetToMove;
    public event Action<Mech> ActionSetToAttack;
    public event Action<StateActionCommittedEventArgs> ActionCommitted;
    public event Action<Tile> HoveredTileSet;
    public event Action<List<TileResult>> PreviewEnabled;
    public event Action PreviewDisabled;
    public event Action<EffectPreview> EffectPreviewShow;

    public void EndGame(GameEndedResult gameEndedResult) => GameEnded?.Invoke(gameEndedResult);

    public void SetActionCancelled(StateActionCancelledEventArgs args) => ActionCancelled?.Invoke(args);
    public void SetActionToMove(StateActionSetToMoveEventArgs args) => ActionSetToMove?.Invoke(args);
    public void SetActionToAttack(Mech mech) => ActionSetToAttack?.Invoke(mech);
    public void SetActionCommitted(StateActionCommittedEventArgs args) => ActionCommitted?.Invoke(args);

    public void SetHoveredTile(Tile tile) => HoveredTileSet?.Invoke(tile);

    public void ShowPreview(List<TileResult> tileResults) => PreviewEnabled?.Invoke(tileResults);

    public void ClearPreview() => PreviewDisabled?.Invoke();

    public void ShowEffectPreview(EffectPreview effectPreview) => EffectPreviewShow?.Invoke(effectPreview);
}
