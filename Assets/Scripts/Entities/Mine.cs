using UnityEngine;
using UnityEngine.Assertions;
using System;
using Framework;

public class Mine : TileHazard
{
    public event Action<Mine> Triggered;

    [SerializeField] private Effect _triggerEffect;

    public override void Initialize(Tile tile, GameboardHelper gameboardHelper)
    {
        base.Initialize(tile, gameboardHelper);

        Assert.IsNotNull(_triggerEffect, "Trigger effect is missing.");

        Tile.OccupantEntered += Trigger;
        Tile.ReceivedHealthChange += Trigger;
    }

    private void Trigger(Tile tile)
    {
        tile.OccupantEntered -= Trigger;
        tile.ReceivedHealthChange -= Trigger;

        Effect.Spawn(_triggerEffect, (effect) =>
        {
            effect.Apply(Helper, new SpawnEffectParameters(tile, tile));
        });

        tile.RemoveHazard(this);

        Triggered.InvokeSafe(this);

        Destroy(gameObject);
    }
}
