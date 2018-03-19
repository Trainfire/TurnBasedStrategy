using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Framework;
using Framework.UI;

public class UIHud : GameEntity
{
    private Gameboard _gameboard;

    [SerializeField] private UIHealthbar _healthbarPrototype;
    private Dictionary<Unit, UIHealthbar> _healthbars;

    private void Awake()
    {
        Assert.IsNotNull(_healthbarPrototype);
        _healthbarPrototype.gameObject.SetActive(false);
        _healthbars = new Dictionary<Unit, UIHealthbar>();
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();

        _gameboard = FindObjectOfType<Gameboard>();

        Assert.IsNotNull(_gameboard);

        if (_gameboard != null)
            _gameboard.UnitAdded += OnUnitAdded;
    }

    private void OnUnitAdded(Unit unit)
    {
        Assert.IsNotNull(unit);
        Assert.IsFalse(_healthbars.ContainsKey(unit));

        unit.Died += OnUnitDied;

        var comp = UIUtility.Add<UIHealthbar>(gameObject.transform, _healthbarPrototype.gameObject);
        comp.Initialize(unit);

        _healthbars.Add(unit, comp);
    }

    private void OnUnitDied(Unit unit)
    {
        Assert.IsTrue(_healthbars.ContainsKey(unit));

        Destroy(_healthbars[unit]);

        _healthbars.Remove(unit);
    }
}
