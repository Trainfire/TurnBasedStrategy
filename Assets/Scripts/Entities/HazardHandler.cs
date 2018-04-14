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
    private struct State
    {
        public bool Triggered { get; set; }

        public State(bool triggered)
        {
            Triggered = triggered;
        }
    }

    private Stack<State> _stateStack;
    private bool _triggered;

    public HazardOnEnterHandler(Hazard hazard) : base(hazard)
    {
        _stateStack = new Stack<State>();
    }

    protected override void OnHazardTriggered(Hazard hazard)
    {
        _triggered = true;
    }

    public override void SaveStateBeforeMove()
    {
        _stateStack.Push(new State(_triggered));
    }

    public override void RestoreStateBeforeMove()
    {
        if (_stateStack.Count == 0)
            return;

        _triggered = _stateStack.Pop().Triggered;
    }

    public override void CommitStateAfterAttack()
    {
        _stateStack.Clear();

        if (Hazard.Data.MaxTriggerCount != -1 && _triggered && Hazard.TriggeredCount >= Hazard.Data.MaxTriggerCount)
            RemoveHazard();

        _triggered = false;
    }
}