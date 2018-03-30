using Framework;

public class EffectPushbackComponent : EffectComponent
{
    protected override void ApplyEffect(ApplyEffectParameters applyEffectParameters)
    {
        applyEffectParameters.Receiver.Occupant.gameObject.GetComponent<UnitPushableComponent>((pushableComponent) =>
        {
            pushableComponent.Push(GridHelper.VectorToDirection(applyEffectParameters.Direction));
        });
    }
}
