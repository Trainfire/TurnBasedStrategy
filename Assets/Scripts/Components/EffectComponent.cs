﻿using UnityEngine;

public enum EffectReceiver
{
    Source,
    Target
}

public abstract class EffectComponent : MonoBehaviour
{
    public struct ApplyEffectParameters
    {
        public GameboardHelper Helper { get; private set; }
        public Tile Receiver { get; private set; }
        public Vector2 Direction { get; private set; }

        public ApplyEffectParameters(GameboardHelper helper, Tile receiver, Vector2 direction)
        {
            Helper = helper;
            Receiver = receiver;
            Direction = direction;
        }
    }

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
        var direction = Vector2.zero;

        switch (_effectDirection)
        {
            case RelativeDirection.Forward: direction = directionToTarget; break;
            case RelativeDirection.Right: direction = -GridHelper.Cross(directionToTarget); break;
            case RelativeDirection.Back: direction = -directionToTarget; break;
            case RelativeDirection.Left: direction = GridHelper.Cross(directionToTarget); break;
        }

        receivingTile = gameboardHelper.GetTile(receivingTile.transform.GetGridPosition() + direction * _relativeOffset);

        if (receivingTile == null || !receivingTile.Occupied)
            return;

        ApplyEffect(new ApplyEffectParameters(gameboardHelper, receivingTile, direction));
    }

    protected abstract void ApplyEffect(ApplyEffectParameters applyEffectParameters);
}