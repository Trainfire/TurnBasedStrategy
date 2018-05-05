using UnityEngine;

public class EnemyData : ScriptableObject
{
    public string Name { get { return _name; } }
    public int MaxHealth { get { return _maxHealth; } }
    public GameObject View { get { return _view; } }
    public WeaponData Weapon { get { return _weapon; } }

    [SerializeField] private string _name;
    [SerializeField] private int _maxHealth;
    [SerializeField] private GameObject _view;
    [SerializeField] private WeaponData _weapon;
}