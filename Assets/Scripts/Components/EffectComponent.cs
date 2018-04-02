using UnityEngine;

public enum EffectReceiver
{
    Source,
    Target
}

public abstract class EffectComponent : MonoBehaviour
{
    public struct ApplyEffectParameters
    {
        public GameboardWorldHelper Helper { get; private set; }
        public Tile Receiver { get; private set; }
        public Vector2 Direction { get; private set; }

        public ApplyEffectParameters(GameboardWorldHelper helper, Tile receiver, Vector2 direction)
        {
            Helper = helper;
            Receiver = receiver;
            Direction = direction;
        }
    }

    protected virtual bool OnlyAffectOccupiedTiles { get { return true; } }

    [SerializeField] private EffectReceiver _effectReceiver;
    [SerializeField] private RelativeDirection _effectDirection;
    [SerializeField] private int _relativeOffset;

    public void Apply(GameboardWorldHelper gameboardHelper, SpawnEffectParameters unitAttackEvent)
    {
        Tile receivingTile = null;

        switch (_effectReceiver)
        {
            case EffectReceiver.Source: receivingTile = unitAttackEvent.Source; break;
            case EffectReceiver.Target: receivingTile = unitAttackEvent.Target; break;
        }

        var directionToTarget = GridHelper.DirectionBetween(unitAttackEvent.Source.transform, unitAttackEvent.Target.transform);
        var direction = Vector2.zero;

        switch (_effectDirection)
        {
            case RelativeDirection.Forward: direction = directionToTarget; break;
            case RelativeDirection.Right: direction = -GridHelper.Cross(directionToTarget); break;
            case RelativeDirection.Back: direction = -directionToTarget; break;
            case RelativeDirection.Left: direction = GridHelper.Cross(directionToTarget); break;
        }

        receivingTile = gameboardHelper.GetTile(receivingTile.transform.GetGridPosition() + direction * _relativeOffset);

        if (receivingTile == null || OnlyAffectOccupiedTiles && receivingTile.Occupant == null)
            return;

        ApplyEffect(new ApplyEffectParameters(gameboardHelper, receivingTile, direction));
    }

    protected abstract void ApplyEffect(ApplyEffectParameters applyEffectParameters);
}