using UnityEngine;

public class WeaponData : ScriptableObject
{
    public int MinRange { get { return _minRange; } }
    public int MaxRange { get { return _maxRange; } }
    public EffectRoot EffectPrototype { get { return _effectPrototype; } }

    [SerializeField] private int _minRange;
    [SerializeField] private int _maxRange;
    [SerializeField] private EffectRoot _effectPrototype;
}