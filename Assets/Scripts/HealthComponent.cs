using System;
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

public class HealthComponent : MonoBehaviour
{
    public event Action<HealthChangeEvent> Changed;
    public event Action<HealthComponent> Died;

    public int Max { get; private set; }
    public int Current { get; private set; }

    public void Initialize(int maxHealth)
    {
        Current = maxHealth;
        Max = maxHealth;
    }

    public void Modify(int delta)
    {
        if (delta == 0)
            return;

        var previousHealth = Current;

        Current = Mathf.Clamp(Current + delta, 0, Mathf.Max(0, Max));

        Changed.InvokeSafe(new HealthChangeEvent(this, previousHealth));

        if (Current == 0)
            Died.InvokeSafe(this);
    }
}