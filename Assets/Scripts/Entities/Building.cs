using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Building : Unit
{
    [SerializeField] private int _health;

    public override void Initialize(GameboardWorldHelper gameboardHelper)
    {
        base.Initialize(gameboardHelper);
        Health.Setup(_health);
    }
}
