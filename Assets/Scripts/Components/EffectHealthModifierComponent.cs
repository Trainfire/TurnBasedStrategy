using UnityEngine;
using Framework;

public class EffectHealthModifierComponent : EffectComponent
{
    [SerializeField] private int _amount;

    protected override void ApplyEffect(ApplyEffectParameters applyEffectParameters)
    {
        applyEffectParameters.Receiver.Occupant.Health.Modify(_amount);
    }
}
