using UnityEngine;
using System.Collections;
using System;

public class UnitMoveAnimator : MonoBehaviour
{
    private Action _onAnimationComplete;
    private Unit _actor;
    private Tile _targetTile;

    public void Animate(Unit actor, Tile targetTile, Action onAnimationComplete)
    {
        _actor = actor;
        _targetTile = targetTile;
        _onAnimationComplete = onAnimationComplete;

        StartCoroutine(ExecuteAnimation());
    }

    private IEnumerator ExecuteAnimation()
    {
        yield return new WaitForSeconds(1f);

        _onAnimationComplete.Invoke();

        _actor = null;
        _targetTile = null;
        _onAnimationComplete = null;
    }
}