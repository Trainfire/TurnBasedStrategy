using System;
using System.Collections.Generic;

public class HazardHandler : IStateHandler
{
    public event Action<HazardHandler> Removed;

    protected Hazard Hazard { get; private set; }

    public HazardHandler(Hazard hazard)
    {
        Hazard = hazard;
        Hazard.Triggered += OnHazardTriggered;
    }

    protected virtual void OnHazardTriggered(Hazard hazard) { }

    protected void RemoveHazard()
    {
        Removed?.Invoke(this);

        Hazard = null;
    }

    public virtual void CommitStateAfterAttack() { }
    public virtual void RestoreStateBeforeMove() { }
    public virtual void SaveStateBeforeMove() { }
}

public class HazardOnEnterHandler : HazardHandler
{
    private StateHandledValue<bool> _triggered;

    public HazardOnEnterHandler(Hazard hazard) : base(hazard)
    {
        _triggered = new StateHandledValue<bool>();
    }

    protected override void OnHazardTriggered(Hazard hazard)
    {
        _triggered.Value = true;
    }

    public override void SaveStateBeforeMove() => _triggered.SaveStateBeforeMove();
    public override void RestoreStateBeforeMove() => _triggered.RestoreStateBeforeMove();

    public override void CommitStateAfterAttack()
    {
        _triggered.CommitStateAfterAttack();

        if (Hazard.Data.MaxTriggerCount != -1 && _triggered.Value && Hazard.TriggeredCount >= Hazard.Data.MaxTriggerCount)
            RemoveHazard();

        _triggered.Value = false;
    }
}