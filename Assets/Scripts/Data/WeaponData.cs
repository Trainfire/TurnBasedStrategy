using UnityEngine;

public enum WeaponType
{
    Invalid,
    Precision,
    Projectile,
}

public class WeaponData : ScriptableObject
{
    public WeaponType WeaponType { get { return _weaponType; } }
    public int MinRange { get { return _minRange; } }
    public int MaxRange { get { return _maxRange; } }
    public Effect EffectPrototype { get { return _effectPrototype; } }
    public Projectile ProjectilePrototype { get { return _projectilePrototype; } }

    [SerializeField] private WeaponType _weaponType;
    [SerializeField] private int _minRange;
    [SerializeField] private int _maxRange;
    [SerializeField] private Effect _effectPrototype;
    [SerializeField] private Projectile _projectilePrototype;
}