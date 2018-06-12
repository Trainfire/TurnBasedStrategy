using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Building : Unit
{
    [SerializeField] private int _health;

    public override void Initialize(Helper gameboardHelper)
    {
        base.Initialize(gameboardHelper);
        Health.Setup(_health);
        Health.Killed += OnHealthKilled;
    }

    private void OnHealthKilled(HealthComponent healthComponent)
    {
        healthComponent.Killed -= OnHealthKilled;
        RemoveSelf();
    }
}
