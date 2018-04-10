using UnityEngine.Assertions;
using Framework;

public class EffectPushbackComponent : EffectComponent
{
    protected override void OnGetPreview(ApplyEffectParameters applyEffectParameters, EffectPreview effectResult)
    {
        // Todo.
    }

    protected override void OnApply(ApplyEffectParameters applyEffectParameters)
    {
        Assert.IsNotNull(applyEffectParameters.Receiver.Occupant);

        applyEffectParameters.Receiver.Occupant.gameObject.GetComponent<UnitPushableComponent>((pushableComponent) =>
        {
            pushableComponent.Push(GridHelper.VectorToDirection(applyEffectParameters.Direction));
        });
    }
}
