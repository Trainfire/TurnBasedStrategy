using UnityEngine;

public class HazardData : ScriptableObject
{
    public string Name { get; private set; }
    public HazardEffectTrigger EffectTrigger { get { return _effectTrigger; } }
    public Effect EffectPrototype { get { return _effectPrototype; } }
    public HazardView ViewPrototype { get { return _viewPrototype; } }
    public bool RemoveOnTrigger { get { return _removeOnTrigger; } }

    [SerializeField] private string _name;
    [SerializeField] private HazardEffectTrigger _effectTrigger;
    [SerializeField] private Effect _effectPrototype;
    [SerializeField] private HazardView _viewPrototype;
    [SerializeField] private bool _removeOnTrigger;
}