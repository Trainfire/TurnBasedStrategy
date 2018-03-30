using UnityEngine;
using UnityEngine.Assertions;
using Framework;

public class EffectHealthModifierComponent : EffectComponent
{
    [SerializeField] private int _amount;

    protected override void ApplyEffect(ApplyEffectParameters applyEffectParameters)
    {
        applyEffectParameters.Receiver.ApplyHealthChange(_amount);
    }
}
