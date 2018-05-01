using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class UnitMoveAnimator : MonoBehaviour
{
    private const float DelayBetweenMoves = 0.1f;

    private enum Axis
    {
        Y,
        X,
    }

    public void Animate(Helper helper, Unit actor, Tile targetTile, Action onAnimationComplete)
    {
        var tilesToTraverse = new List<Tile>();

        var yTiles = GetTilesInDirection(helper, actor.Tile, targetTile, Axis.Y);

        var xStartTile = yTiles.Count == 0 ? actor.Tile : yTiles.Last();
        var xTiles = GetTilesInDirection(helper, xStartTile, targetTile, Axis.X);

        tilesToTraverse.AddRange(yTiles);
        tilesToTraverse.AddRange(xTiles);

        StartCoroutine(ExecuteAnimation(actor, tilesToTraverse, onAnimationComplete));
    }

    private List<Tile> GetTilesInDirection(Helper helper, Tile startTile, Tile targetTile, Axis axis)
    {
        var results = new List<Tile>();

        var direction = Vector2.zero;
        var distance = 0f;

        switch (axis)
        {
            case Axis.Y:
                direction = new Vector2(0f, targetTile.transform.GetGridPosition().y - startTile.transform.GetGridPosition().y).normalized;
                distance = targetTile.transform.GetGridPosition().y - startTile.transform.GetGridPosition().y;
                break;
            case Axis.X:
                direction = new Vector2(targetTile.transform.GetGridPosition().x - startTile.transform.GetGridPosition().x, 0f).normalized;
                distance = targetTile.transform.GetGridPosition().x - startTile.transform.GetGridPosition().x;
                break;
        }

        // Start one tile in the direction.
        var lastPosition = startTile.transform.GetGridPosition() + direction;

        for (int i = 0; i < (int)Mathf.Abs(distance); i++)
        {
            var tile = helper.GetTile(lastPosition + (direction * i));
            results.Add(tile);
        }

        return results;
    }

    private IEnumerator ExecuteAnimation(Unit actor, List<Tile> tiles, Action onAnimationComplete)
    {
        foreach (var tile in tiles)
        {
            yield return new WaitForSeconds(DelayBetweenMoves);
            actor.transform.SetGridPosition(tile.transform.GetGridPosition());
        }

        onAnimationComplete.Invoke();
    }
}