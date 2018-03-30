using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;
using Framework;

public struct SpawnEffectParameters
{
    public Tile Source { get; private set; }
    public Tile Target { get; private set; }

    public SpawnEffectParameters(Tile source, Tile target)
    {
        Source = source;
        Target = target;
    }
}

public class Effect : MonoBehaviour
{
    private List<EffectComponent> _effects;

    private void Awake()
    {
        _effects = new List<EffectComponent>();
        GetComponents(_effects);
    }

    public void Apply(GameboardHelper gameboardHelper, SpawnEffectParameters spawnEffectParameters)
    {
        _effects.ForEach(x => x.Apply(gameboardHelper, spawnEffectParameters));
        Destroy(gameObject);
    }

    public static Effect Spawn(Effect prototype)
    {
        Assert.IsNotNull(prototype);
        return Instantiate<Effect>(prototype);
    }

    public static void Spawn(Effect prototype, Action<Effect> onSpawn)
    {
        Assert.IsNotNull(prototype);
        onSpawn(Instantiate<Effect>(prototype));
    }
}