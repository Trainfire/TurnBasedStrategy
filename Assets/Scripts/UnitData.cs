using UnityEngine;
using UnityEditor;

public class UnitData : ScriptableObject
{
    public string Name { get { return _name; } }
    public int MaxHealth { get { return _maxHealth; } }
    public int MovementRange { get { return _movementRange; } }
    public GameObject Prefab { get { return _prefab; } }
    public WeaponData DefaultPrimaryWeapon { get { return _defaultPrimaryWeapon; } }

    [SerializeField] private string _name;
    [SerializeField] private int _maxHealth;
    [SerializeField] private int _movementRange;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private WeaponData _defaultPrimaryWeapon;
}