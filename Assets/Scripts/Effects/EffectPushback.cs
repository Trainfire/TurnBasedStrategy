using System;
using UnityEngine;

public class EffectPushback : EffectBase
{
    protected override void ApplyEffect(GameboardHelper gameboardHelper, Tile sourceTile)
    {
        sourceTile.Occupant.Push(GridHelper.VectorToDirection(AttackDirection));
    }
}
