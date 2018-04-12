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

public class EffectPreview
{
    public IReadOnlyDictionary<Tile, int> HealthChanges { get { return _healthChanges; } }
    public IReadOnlyDictionary<Tile, WorldDirection> Pushes { get { return _pushes; } }
    public IReadOnlyCollection<Tile> Collisions { get { return _collisions; } }

    private Dictionary<Tile, int> _healthChanges;
    private Dictionary<Tile, WorldDirection> _pushes;
    private HashSet<Tile> _collisions;

    public EffectPreview()
    {
        _healthChanges = new Dictionary<Tile, int>();
        _pushes = new Dictionary<Tile, WorldDirection>();
        _collisions = new HashSet<Tile>();
    }

    public void RegisterHealthChange(Tile tile, int healthChangeDelta)
    {
        if (!_healthChanges.ContainsKey(tile))
            _healthChanges.Add(tile, 0);

        _healthChanges[tile] += healthChangeDelta;
    }

    public void RegisterPush(Tile tile, WorldDirection worldDirection)
    {
        if (_pushes.ContainsKey(tile))
        {
            DebugEx.LogWarning<EffectPreview>("Trying to register a push on a tile that already has a push registered.");
        }
        else
        {
            _pushes.Add(tile, worldDirection);
        }
    }

    public void RegisterCollision(Tile tile)
    {
        if (_collisions.Contains(tile))
        {
            DebugEx.LogWarning<EffectPreview>("Trying to register a collision on a tile that already has a collision registered.");
        }
        else
        {
            _collisions.Add(tile);
        }
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

    public void Apply(GameboardWorldHelper gameboardHelper, SpawnEffectParameters spawnEffectParameters)
    {
        _effects.ForEach(x => x.Apply(gameboardHelper, spawnEffectParameters));
        Destroy(gameObject);
    }

    private EffectPreview GetPreview(GameboardWorldHelper helper, SpawnEffectParameters parameters)
    {
        var effectResult = new EffectPreview();
        _effects.ForEach(x => x.GetPreview(effectResult, helper, parameters));
        return effectResult;
    }

    public static EffectPreview GetPreview(Effect prototype, GameboardWorldHelper helper, SpawnEffectParameters spawnEffectParameters)
    {
        var effectPreview = new EffectPreview();

        if (prototype == null)
        {
            DebugEx.LogWarning<GameboardWorldHelper>("Cannot get effect preview as prototype is null.");
            return effectPreview;
        }

        if (spawnEffectParameters.Source == null || spawnEffectParameters.Target == null)
        {
            DebugEx.LogWarning<GameboardWorldHelper>("Cannot get effect result as source and/or target tile is null.");
            return effectPreview;
        }

        var effectInstance = Spawn(prototype);

        effectPreview = effectInstance.GetPreview(helper, spawnEffectParameters);

        Destroy(effectInstance.gameObject);

        return effectPreview;
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