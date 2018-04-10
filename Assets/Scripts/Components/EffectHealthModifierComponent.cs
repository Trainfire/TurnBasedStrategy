using UnityEngine;
using UnityEngine.Assertions;
using Framework;

public class EffectHealthModifierComponent : EffectComponent
{
    [SerializeField] private int _amount;

    protected override void OnGetPreview(ApplyEffectParameters applyEffectParameters, EffectPreview effectResult)
    {
        effectResult.RegisterHealthChange(applyEffectParameters.Receiver, _amount);
    }

    protected override void OnApply(ApplyEffectParameters applyEffectParameters)
    {
        applyEffectParameters.Receiver.ApplyHealthChange(_amount);
    }
}
