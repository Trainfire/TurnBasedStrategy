using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Framework;

public class HealthChangeEvent
{
    public HealthComponent Health { get; private set; }
    public int PreviousHealth { get; private set; }

    public HealthChangeEvent(HealthComponent sender, int oldHealth)
    {
        Health = sender;
        PreviousHealth = oldHealth;
    }
}

public class HealthComponent : MonoBehaviour, IStateHandler
{
    public event Action<HealthChangeEvent> Changed;
    public event Action<HealthComponent> Killed;

    public bool Invincible { get; set; }
    public int Max { get; private set; }
    public int Current { get; private set; }

    private Stack<int> _previousHealthValues;

    private void Awake()
    {
        _previousHealthValues = new Stack<int>();
    }

    public void Setup(int maxHealth)
    {
        Current = maxHealth;
        Max = maxHealth;
    }

    public void Modify(int delta)
    {
        if (delta == 0 || Invincible)
            return;

        Set(Current + delta);
    }

    private void Set(int value)
    {
        var previousHealth = Current;

        Current = Mathf.Clamp(value, 0, Mathf.Max(0, Max));

        Changed.InvokeSafe(new HealthChangeEvent(this, previousHealth));

        if (Current == 0)
            Killed.InvokeSafe(this);
    }

    void IStateHandler.Record()
    {
        _previousHealthValues.Push(Current);
    }

    void IStateHandler.Undo()
    {
        Assert.IsFalse(_previousHealthValues.Count == 0);
        Set(_previousHealthValues.Pop(), true);
    }

    void IStateHandler.Commit()
    {
        _previousHealthValues.Clear();
    }
}