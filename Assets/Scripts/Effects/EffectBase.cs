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

        receivingTile = gameboardHelper.GetTile(receivingTile.Position + GetVectorFromDirection(_effectDirection));

        if (_effectReceiver == EffectReceiver.Source)
        {
            ApplyEffect(gameboardHelper, gameboardHelper.GetTile(unitAttackEvent.Source));
        }
        else
        {
            ApplyEffect(gameboardHelper, unitAttackEvent.TargetTile);
        }
    }

    protected abstract void ApplyEffect(GameboardHelper gameboardHelper, Tile sourceTile);

    private Vector2 GetVectorFromDirection(EffectDirection effectDirection)
    {
        switch (_effectDirection)
        {
            case EffectDirection.Forward: return Vector2.up;
            case EffectDirection.Right: return Vector2.right;
            case EffectDirection.Back: return Vector2.down;
            case EffectDirection.Left: return Vector2.left;
        }

        return Vector2.zero;
    }
}