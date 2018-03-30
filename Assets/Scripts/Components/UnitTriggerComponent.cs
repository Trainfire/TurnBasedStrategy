using UnityEngine;
using System;
using Framework;

public struct UnitTriggerComponentEvent
{
    public UnitTriggerComponent Sender { get; private set; }
    public Tile SourceTile { get; private set; }

    public UnitTriggerComponentEvent(UnitTriggerComponent sender, Tile sourceTile)
    {
        Sender = sender;
        SourceTile = sourceTile;
    }
}

public class UnitTriggerComponent : UnitComponent
{
    public event Action<UnitTriggerComponentEvent> Triggered;

    public void Trigger(Tile source)
    {
        Triggered.InvokeSafe(new UnitTriggerComponentEvent(this, source));
    }
}