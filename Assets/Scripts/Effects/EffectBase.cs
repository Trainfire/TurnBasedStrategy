using UnityEngine;

public enum EffectReceiver
{
    Source,
    Target
}

public abstract class EffectBase : MonoBehaviour
{
    protected Vector2 AttackDirection { get; private set; }

    [SerializeField] private EffectReceiver _effectReceiver;
    [SerializeField] private RelativeDirection _effectDirection;
    [SerializeField] private int _relativeOffset;

    public void Apply(GameboardHelper gameboardHelper, UnitAttackEvent unitAttackEvent)
    {
        Tile receivingTile = null;

        switch (_effectReceiver)
        {
            case EffectReceiver.Source: receivingTile = gameboardHelper.GetTile(unitAttackEvent.Source); break;
            case EffectReceiver.Target: receivingTile = unitAttackEvent.TargetTile; break;
        }

        var directionToTarget = GridHelper.DirectionBetween(unitAttackEvent.Source.transform, unitAttackEvent.TargetTile.transform);

        switch (_effectDirection)
        {
            case RelativeDirection.Forward: AttackDirection = directionToTarget; break;
            case RelativeDirection.Right: AttackDirection = -GridHelper.Cross(directionToTarget); break;
            case RelativeDirection.Back: AttackDirection = -directionToTarget; break;
            case RelativeDirection.Left: AttackDirection = GridHelper.Cross(directionToTarget); break;
        }

        receivingTile = gameboardHelper.GetTile(receivingTile.transform.GetGridPosition() + AttackDirection * _relativeOffset);

        if (receivingTile == null || !receivingTile.Occupied)
            return;

        ApplyEffect(gameboardHelper, receivingTile);
    }

    protected abstract void ApplyEffect(GameboardHelper gameboardHelper, Tile sourceTile);
}