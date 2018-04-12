using Framework;
using System;
using UnityEngine;
using UnityEngine.Assertions;

public enum HazardEffectTrigger
{
    Unassigned,
    OnEnter,
    OnStay,
}

public class Hazard : IStateHandler
{
    public event Action<Hazard> Triggered;

    public HazardData Data { get; private set; }
    public Tile Tile { get; private set; }

    private GameboardWorldHelper _helper;

    public Hazard(HazardData hazardData, Tile tile, GameboardWorldHelper helper)
    {
        Data = hazardData;
        Tile = tile;
        _helper = helper;

        Assert.IsNotNull(Data.ViewPrototype, "View is missing.");
        var view = GameObject.Instantiate<HazardView>(Data.ViewPrototype);
        view.Initialize(this);
    }

    public void Trigger()
    {
        Effect.Spawn(Data.EffectPrototype, (effect) => effect.Apply(_helper, new SpawnEffectParameters(Tile, Tile)));

        Triggered?.Invoke(this);
    }

    void IStateHandler.SaveStateBeforeMove()
    {
        // TODO.
    }

    void IStateHandler.RestoreStateBeforeMove()
    {
        // TODO.
    }

    void IStateHandler.CommitStateAfterAttack()
    {
        // TODO.
    }
}