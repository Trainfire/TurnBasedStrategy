using UnityEngine;
using UnityEditor;

public class MechData : ScriptableObject
{
    public string Name { get { return _name; } }
    public int MaxHealth { get { return _maxHealth; } }
    public int MovementRange { get { return _movementRange; } }
    public GameObject View { get { return _view; } }
    public WeaponData DefaultPrimaryWeapon { get { return _defaultPrimaryWeapon; } }
    public WeaponData DefaultSecondaryWeapon { get { return _defaultSecondaryWeapon; } }

    [SerializeField] private string _name;
    [SerializeField] private int _maxHealth;
    [SerializeField] private int _movementRange;
    [SerializeField] private GameObject _view;
    [SerializeField] private WeaponData _defaultPrimaryWeapon;
    [SerializeField] private WeaponData _defaultSecondaryWeapon;
}