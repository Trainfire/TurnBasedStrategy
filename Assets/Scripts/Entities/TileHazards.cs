using Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class TileHazards : MonoBehaviour, IStateHandler
{
    private Tile _tile;
    private GameboardWorldHelper _helper;

    private Dictionary<HazardEffectTrigger, Hazard> _hazards;

    public void Initialize(Tile tile, GameboardWorldHelper helper)
    {
        _tile = tile;
        _tile.OccupantEntered += OnOccupantEnteredTile;

        _helper = helper;

        _hazards = new Dictionary<HazardEffectTrigger, Hazard>();
    }

    public void Add(HazardData hazardData)
    {
        DebugEx.Log<Tile>($"Added hazard: {hazardData.name} to tile {_tile.name}.");

        Assert.IsFalse(_hazards.ContainsKey(hazardData.EffectTrigger), "Only one hazard of each trigger type is allowed.");

        var hazard = _tile.gameObject.AddComponent<Hazard>();
        hazard.Initialize(hazardData, _tile, _helper);
        hazard.Removed += OnHazardRemoved;

        _hazards.Add(hazard.Data.EffectTrigger, hazard);
    }

    public void Remove(Hazard hazard)
    {
        Assert.IsTrue(_hazards.ContainsValue(hazard));

        DebugEx.Log<Tile>($"Removed hazard: {hazard.Data.Name}");

        _hazards.Remove(hazard.Data.EffectTrigger);

        GameObject.Destroy(hazard);
    }

    public EffectPreview GetEffectPreviewOnEnter(Unit movingUnit)
    {
        if (ShouldDefaultEffectPreview(HazardEffectTrigger.OnEnter))
            return new EffectPreview();

        if (movingUnit == null)
        {
            DebugEx.LogWarning<TileHazards>("Cannot return effect preview as the specified unit is null.");
            return new EffectPreview();
        }

        var movingUnitTile = _helper.GetTile(movingUnit);
        if (movingUnitTile == null)
            return new EffectPreview();

        return Effect.GetPreview(_hazards[HazardEffectTrigger.OnEnter].Data.EffectPrototype, _helper, new SpawnEffectParameters(movingUnitTile, _tile));
    }

    public EffectPreview GetEffectPreview(HazardEffectTrigger effectTriggerType)
    {
        if (ShouldDefaultEffectPreview(effectTriggerType))
            return new EffectPreview();

        return Effect.GetPreview(_hazards[effectTriggerType].Data.EffectPrototype, _helper, new SpawnEffectParameters(_tile, _tile));
    }

    private bool ShouldDefaultEffectPreview(HazardEffectTrigger effectTriggerType)
    {
        if (!_hazards.ContainsKey(effectTriggerType))
            return true;

        if (_hazards[effectTriggerType].Data.EffectPrototype == null)
        {
            DebugEx.LogWarning<Hazard>("Cannot return effect preview as the effect prototype is missing for this hazard.");
            return true;
        }

        return false;
    }

    private void OnOccupantEnteredTile(Tile tile)
    {
        if (_hazards.ContainsKey(HazardEffectTrigger.OnEnter))
            _hazards[HazardEffectTrigger.OnEnter].Trigger();
    }

    private void OnHazardRemoved(Hazard hazard) => Remove(hazard);

    void IStateHandler.SaveStateBeforeMove() => _hazards.Values.Cast<IStateHandler>().ToList().ForEach(x => x.SaveStateBeforeMove());
    void IStateHandler.RestoreStateBeforeMove() => _hazards.Values.Cast<IStateHandler>().ToList().ForEach(x => x.RestoreStateBeforeMove());
    void IStateHandler.CommitStateAfterAttack() => _hazards.Values.Cast<IStateHandler>().ToList().ForEach(x => x.CommitStateAfterAttack());
}