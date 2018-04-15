using UnityEngine.Assertions;

public class EffectPushbackComponent : EffectComponent
{
    private const int DamageOnCollide = -1;

    protected override void OnGetPreview(ApplyEffectParameters applyEffectParameters, EffectPreview effectPreview)
    {
        var pushableComponent = applyEffectParameters.GetComponentFromOccupant<UnitPushableComponent>();
        if (pushableComponent != null)
        {
            var vectorToDirection = GridHelper.VectorToDirection(applyEffectParameters.Direction);

            var collisionResults = applyEffectParameters.Helper.GetCollisions(pushableComponent.Unit, vectorToDirection);
            collisionResults.ForEach(result =>
            {
                effectPreview.RegisterHealthChange(result, UnitPushableComponent.DamageOnCollision);
                effectPreview.RegisterCollision(result);
            });

            effectPreview.RegisterPush(applyEffectParameters.Receiver, vectorToDirection);
        }
    }

    protected override void OnApply(ApplyEffectParameters applyEffectParameters)
    {
        var pushableComponent = applyEffectParameters.GetComponentFromOccupant<UnitPushableComponent>();
        if (pushableComponent != null)
            pushableComponent.Push(GridHelper.VectorToDirection(applyEffectParameters.Direction));
    }
}
