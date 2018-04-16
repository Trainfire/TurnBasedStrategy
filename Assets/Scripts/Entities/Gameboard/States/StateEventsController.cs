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
    event Action<Mech> ActionSetToMove;
    event Action<Mech> ActionSetToAttack;
    event Action<Tile> HoveredTileSet;
    event Action<List<TileResult>> TilesPreviewShowed;
    event Action TilesPreviewCleared;
    event Action<EffectPreview> EffectPreviewShow;
}

public class StateEventsController : MonoBehaviour, IStateEvents
{
    public event Action<GameEndedResult> GameEnded;
    public event Action<Mech> ActionSetToMove;
    public event Action<Mech> ActionSetToAttack;
    public event Action<Tile> HoveredTileSet;
    public event Action<List<TileResult>> TilesPreviewShowed;
    public event Action TilesPreviewCleared;
    public event Action<EffectPreview> EffectPreviewShow;

    public void EndGame(GameEndedResult gameEndedResult) => GameEnded?.Invoke(gameEndedResult);

    public void SetActionToMove(Mech mech) => ActionSetToMove?.Invoke(mech);
    public void SetActionToAttack(Mech mech) => ActionSetToAttack?.Invoke(mech);

    public void SetHoveredTile(Tile tile) => HoveredTileSet?.Invoke(tile);

    public void ShowTilesPreview(List<TileResult> tileResults) => TilesPreviewShowed?.Invoke(tileResults);
    public void ClearTilesPreview() => TilesPreviewCleared?.Invoke();

    public void ShowEffectPreview(EffectPreview effectPreview) => EffectPreviewShow?.Invoke(effectPreview);
}
