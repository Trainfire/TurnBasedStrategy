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
    public bool CanRestore { get { return Current != 0 && Current != Max && !Invincible; } }
    public int Max { get; private set; }
    public int Current { get; private set; }

    private Stack<int> _undoStack;

    private void Awake()
    {
        _undoStack = new Stack<int>();
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

        if (Current == 0 && _undoStack.Count == 0)
            Killed.InvokeSafe(this);
    }

    void IStateHandler.SaveStateBeforeMove()
    {
        _undoStack.Push(Current);
    }

    void IStateHandler.RestoreStateBeforeMove()
    {
        Assert.IsFalse(_undoStack.Count == 0);
        Set(_undoStack.Pop());
    }

    void IStateHandler.CommitStateAfterAttack()
    {
        _undoStack.Clear();

        if (Current == 0)
            Killed.InvokeSafe(this);
    }
}