using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class UnitMoveAnimator : MonoBehaviour
{
    private const float DelayBetweenMoves = 0.1f;

    public void Animate(Helper helper, Unit actor, Tile targetTile, Action onAnimationComplete)
    {
        var path = helper.GetPath(actor.Tile, targetTile);
        StartCoroutine(ExecuteAnimation(actor, path, onAnimationComplete));
    }

    private IEnumerator ExecuteAnimation(Unit actor, List<TileResult> path, Action onAnimationComplete)
    {
        foreach (var tileResult in path)
        {
            yield return new WaitForSeconds(DelayBetweenMoves);
            actor.transform.SetGridPosition(tileResult.Tile.transform.GetGridPosition());
        }

        onAnimationComplete.Invoke();
    }
}