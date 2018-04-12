using Framework;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class TileHazards
{
    private Tile _tile;
    private GameboardWorldHelper _helper;

    private Dictionary<HazardEffectTrigger, Hazard> _hazards;

    public TileHazards(Tile tile, GameboardWorldHelper helper)
    {
        _tile = tile;
        _tile.OccupantEntered += OnOccupantEnteredTile;

        _helper = helper;

        _hazards = new Dictionary<HazardEffectTrigger, Hazard>();
    }

    public void Add(HazardData hazardData)
    {
        DebugEx.Log<Tile>("Added hazard: {0}", hazardData.Name);

        Assert.IsFalse(_hazards.ContainsKey(hazardData.EffectTrigger), "Only one hazard of each trigger type is allowed.");

        var hazard = new Hazard(hazardData, _tile, _helper);
        _hazards.Add(hazard.Data.EffectTrigger, hazard);
    }

    public void Remove(Hazard hazard)
    {
        Assert.IsTrue(_hazards.ContainsValue(hazard));

        DebugEx.Log<Tile>("Removed hazard: {0}", hazard.Data.Name);

        _hazards.Remove(hazard.Data.EffectTrigger);
    }

    public EffectPreview GetEffectPreview(HazardEffectTrigger effectTriggerType)
    {
        if (!_hazards.ContainsKey(effectTriggerType))
            return new EffectPreview();

        var hazardEffect = _hazards[effectTriggerType].Data.EffectPrototype;
        if (hazardEffect == null)
        {
            DebugEx.LogWarning<Hazard>("Cannot return effect preview as the effect prototype is missing for this hazard.");
            return new EffectPreview();
        }

        return Effect.GetPreview(hazardEffect, _helper, new SpawnEffectParameters(_tile, _tile));
    }

    private void OnOccupantEnteredTile(Tile tile)
    {
        if (_hazards.ContainsKey(HazardEffectTrigger.OnEnter))
            _hazards[HazardEffectTrigger.OnEnter].Trigger();
    }
}