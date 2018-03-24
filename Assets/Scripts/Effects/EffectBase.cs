using UnityEngine;

public enum EffectReceiver
{
    Source,
    Target
}

public enum EffectDirection
{
    Forward,
    Right,
    Back,
    Left,
}

public abstract class EffectBase : MonoBehaviour
{
    [SerializeField] private EffectReceiver _effectReceiver;
    [SerializeField] private EffectDirection _effectDirection;
    [SerializeField] private int _relativeOffset;

    public void Apply(GameboardHelper gameboardHelper, UnitAttackEvent unitAttackEvent)
    {
        Tile receivingTile = null;

        switch (_effectReceiver)
        {
            case EffectReceiver.Source: receivingTile = gameboardHelper.GetTile(unitAttackEvent.Source); break;
            case EffectReceiver.Target: receivingTile = unitAttackEvent.TargetTile; break;
        }

        var attackDirection = (unitAttackEvent.TargetTile.Position - unitAttackEvent.Source.Position).normalized;

        var offsetDirection = Vector2.zero;
        switch (_effectDirection)
        {
            case EffectDirection.Forward: offsetDirection = attackDirection; break;
            case EffectDirection.Right: offsetDirection = Vector3.Cross(attackDirection, Vector3.up); break;
            case EffectDirection.Back: offsetDirection = -attackDirection; break;
            case EffectDirection.Left: offsetDirection = -Vector3.Cross(attackDirection, Vector3.up); break;
        }

        receivingTile = gameboardHelper.GetTile(receivingTile.Position + offsetDirection * _relativeOffset);

        if (_effectReceiver == EffectReceiver.Source)
        {
            ApplyEffect(gameboardHelper, receivingTile);
        }
        else
        {
            ApplyEffect(gameboardHelper, receivingTile);
        }
    }

    protected abstract void ApplyEffect(GameboardHelper gameboardHelper, Tile sourceTile);
}