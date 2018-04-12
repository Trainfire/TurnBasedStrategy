using UnityEngine;
using UnityEngine.Assertions;
using Framework;

public class EffectSpawnHazardComponent : EffectComponent
{
    [SerializeField] private HazardData _hazardData;

    protected override bool OnlyAffectOccupiedTiles { get { return false; } }

    protected override void OnGetPreview(ApplyEffectParameters applyEffectParameters, EffectPreview effectResult)
    {
        // Todo.
    }

    protected override void OnApply(ApplyEffectParameters applyEffectParameters)
    {
        Assert.IsNotNull(_hazardData);
        applyEffectParameters.Receiver.Hazards.Add(_hazardData);
    }
}
