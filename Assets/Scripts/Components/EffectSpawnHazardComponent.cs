using UnityEngine;
using UnityEngine.Assertions;
using Framework;

public class EffectSpawnHazardComponent : EffectComponent
{
    [SerializeField] private TileHazard _hazardPrototype;

    protected override bool OnlyAffectOccupiedTiles { get { return false; } }

    protected override void ApplyEffect(ApplyEffectParameters applyEffectParameters)
    {
        Assert.IsNotNull(_hazardPrototype);
        applyEffectParameters.Receiver.AddHazard(_hazardPrototype);
    }
}
