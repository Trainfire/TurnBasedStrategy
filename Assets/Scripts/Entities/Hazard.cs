using Framework;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

public enum HazardEffectTrigger
{
    Unassigned,
    OnEnter,
    OnStay,
}

public class Hazard : MonoBehaviour, IStateHandler
{
    public event Action<Hazard> Triggered;
    public event Action<Hazard> Removed;

    public HazardData Data { get; private set; }
    public Tile Tile { get; private set; }
    public int TriggeredCount { get; private set; }

    private GameboardWorldHelper _helper;

    private HazardHandler _hazardHandler;
    private Stack<int> _previousTriggerCounts;

    public void Initialize(HazardData hazardData, Tile tile, GameboardWorldHelper helper)
    {
        Data = hazardData;
        Tile = tile;
        _helper = helper;

        Assert.IsNotNull(Data.ViewPrototype, "View is missing.");
        var view = GameObject.Instantiate<HazardView>(Data.ViewPrototype);
        view.Initialize(this);

        _hazardHandler = hazardData.EffectTrigger == HazardEffectTrigger.OnEnter ? new HazardOnEnterHandler(this) : new HazardHandler(this);
        _hazardHandler.Removed += OnHazardHandlerRemoved;

        _previousTriggerCounts = new Stack<int>();
    }

    private void OnHazardHandlerRemoved(HazardHandler hazardHandler)
    {
        hazardHandler.Removed -= OnHazardHandlerRemoved;
        Removed?.Invoke(this);
    }

    public virtual void Trigger()
    {
        Effect.Spawn(Data.EffectPrototype, (effect) => effect.Apply(_helper, new SpawnEffectParameters(Tile, Tile)));
        TriggeredCount++;
        Triggered?.Invoke(this);
    }

    void IStateHandler.SaveStateBeforeMove()
    {
        _hazardHandler.SaveStateBeforeMove();

        _previousTriggerCounts.Push(TriggeredCount);
    }
    void IStateHandler.RestoreStateBeforeMove()
    {
        _hazardHandler.RestoreStateBeforeMove();

        if (_previousTriggerCounts.Count == 0)
            return;

        TriggeredCount = _previousTriggerCounts.Pop();
    }

    void IStateHandler.CommitStateAfterAttack()
    {
        _previousTriggerCounts.Clear();

        _hazardHandler.CommitStateAfterAttack();
    }
}