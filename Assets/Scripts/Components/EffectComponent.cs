using UnityEngine;
using System;

public enum EffectReceiver
{
    Source,
    Target
}

public abstract class EffectComponent : MonoBehaviour
{
    public class ApplyEffectParameters
    {
        public Helper Helper { get; private set; }
        public Tile Receiver { get; private set; }
        public Vector2 Direction { get; private set; }

        public ApplyEffectParameters(Helper helper, Tile receiver, Vector2 direction)
        {
            Helper = helper;
            Receiver = receiver;
            Direction = direction;
        }

        public T GetComponentFromOccupant<T>(Action<T> onGet = null) where T : MonoBehaviour
        {
            if (Receiver != null && Receiver.Occupant != null)
                return Receiver.Occupant.gameObject.GetComponent<T>();
            return null;
        }
    }

    protected virtual bool OnlyAffectOccupiedTiles { get { return false; } }

    [SerializeField] private bool _affectUserTile;
    [SerializeField] private EffectReceiver _effectReceiver;
    [SerializeField] private RelativeDirection _effectDirection;
    [SerializeField] private int _relativeOffset;

    public void GetPreview(EffectPreview effectPreview, Helper gameboardHelper, SpawnEffectParameters spawnEffectParameters)
    {
        var applyEffectParameters = GetApplyEffectParameters(gameboardHelper, spawnEffectParameters);

        if (applyEffectParameters != null)
            OnGetPreview(applyEffectParameters, effectPreview);
    }

    public void Apply(Helper gameboardHelper, SpawnEffectParameters spawnEffectParameters)
    {
        var applyEffectParameters = GetApplyEffectParameters(gameboardHelper, spawnEffectParameters);

        if (applyEffectParameters != null)
            OnApply(applyEffectParameters);
    }

    private ApplyEffectParameters GetApplyEffectParameters(Helper gameboardHelper, SpawnEffectParameters spawnEffectParameters)
    {
        Tile receivingTile = null;

        switch (_effectReceiver)
        {
            case EffectReceiver.Source: receivingTile = spawnEffectParameters.Source; break;
            case EffectReceiver.Target: receivingTile = spawnEffectParameters.Target; break;
        }

        var directionToTarget = GridHelper.DirectionBetween(spawnEffectParameters.Source.transform, spawnEffectParameters.Target.transform);
        var direction = Vector2.zero;

        switch (_effectDirection)
        {
            case RelativeDirection.Forward: direction = directionToTarget; break;
            case RelativeDirection.Right: direction = -GridHelper.Cross(directionToTarget); break;
            case RelativeDirection.Back: direction = -directionToTarget; break;
            case RelativeDirection.Left: direction = GridHelper.Cross(directionToTarget); break;
        }

        receivingTile = gameboardHelper.GetTile(receivingTile.transform.GetGridPosition() + direction * _relativeOffset);

        if (receivingTile == null || OnlyAffectOccupiedTiles && receivingTile.Occupant == null || receivingTile == spawnEffectParameters.Source && !_affectUserTile)
            return null;

        return new ApplyEffectParameters(gameboardHelper, receivingTile, direction);
    }

    protected abstract void OnGetPreview(ApplyEffectParameters applyEffectParameters, EffectPreview effectResult);
    protected abstract void OnApply(ApplyEffectParameters applyEffectParameters);
}