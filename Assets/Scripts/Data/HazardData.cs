using UnityEngine;

public class HazardData : ScriptableObject
{
    public string Name { get { return _name; } }
    public HazardEffectTrigger EffectTrigger { get { return _effectTrigger; } }
    public Effect EffectPrototype { get { return _effectPrototype; } }
    public HazardView ViewPrototype { get { return _viewPrototype; } }
    public int Lifetime { get { return _lifetime; } }
    public int MaxTriggerCount { get { return _maxTriggerCount; } }

    [SerializeField] private string _name;
    [SerializeField] private HazardEffectTrigger _effectTrigger;
    [SerializeField] private Effect _effectPrototype;
    [SerializeField] private HazardView _viewPrototype;
    [SerializeField] private int _lifetime;
    [SerializeField] private int _maxTriggerCount;
}