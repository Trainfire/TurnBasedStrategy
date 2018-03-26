﻿using System;
using UnityEngine;

public class EffectHealthModifier : EffectBase
{
    [SerializeField] private int _amount;

    protected override void ApplyEffect(GameboardHelper gameboardHelper, Tile sourceTile)
    {
        sourceTile.Occupant.Health.Modify(_amount);
    }
}
