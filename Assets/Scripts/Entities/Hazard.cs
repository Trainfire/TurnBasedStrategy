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
    public int TriggeredCount { get { return _triggeredCount.Value; } }

    private Helper _helper;

    private HazardHandler _hazardHandler;
    private StateHandledValue<int> _triggeredCount;
    private List<IStateHandler> _stateHandlers;

    public void Initialize(HazardData hazardData, Tile tile, Helper helper)
    {
        Data = hazardData;
        Tile = tile;
        _helper = helper;

        Assert.IsNotNull(Data.ViewPrototype, "View is missing.");
        var view = GameObject.Instantiate<HazardView>(Data.ViewPrototype);
        view.Initialize(this);

        _hazardHandler = hazardData.EffectTrigger == HazardEffectTrigger.OnEnter ? new HazardOnEnterHandler(this) : new HazardHandler(this);
        _hazardHandler.Removed += OnHazardHandlerRemoved;

        _triggeredCount = new StateHandledValue<int>();

        _stateHandlers = new List<IStateHandler>();
        _stateHandlers.Add(_hazardHandler);
        _stateHandlers.Add(_triggeredCount);
    }

    private void OnHazardHandlerRemoved(HazardHandler hazardHandler)
    {
        hazardHandler.Removed -= OnHazardHandlerRemoved;
        Removed?.Invoke(this);
    }

    public virtual void Trigger()
    {
        Effect.Spawn(Data.EffectPrototype, (effect) => effect.Apply(_helper, new SpawnEffectParameters(Tile, Tile)));
        _triggeredCount.Value++;
        Triggered?.Invoke(this);
    }

    void IStateHandler.SaveStateBeforeMove() => _stateHandlers.ForEach(handler => handler.SaveStateBeforeMove());
    void IStateHandler.RestoreStateBeforeMove() => _stateHandlers.ForEach(handler => handler.RestoreStateBeforeMove());
    void IStateHandler.CommitStateAfterAttack() => _stateHandlers.ForEach(handler => handler.CommitStateAfterAttack());
}