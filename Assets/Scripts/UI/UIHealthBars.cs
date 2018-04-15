using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using Framework;
using Framework.UI;

public class UIHealthBars : MonoBehaviour
{
    [SerializeField] private UIHealthbar _healthbarPrototype;
    private Dictionary<Unit, UIHealthbar> _healthbars;

    private GameboardObjects _gameboardObjects;
    private GameboardState _gameboardState;

    private void Awake()
    {
        Assert.IsNotNull(_healthbarPrototype);
        _healthbarPrototype.gameObject.SetActive(false);
        _healthbars = new Dictionary<Unit, UIHealthbar>();
    }

    public void Initialize(GameboardObjects gameboardObjects, GameboardState gameboardState)
    {
        _gameboardObjects = gameboardObjects;
        _gameboardObjects.UnitAdded += AddHealthBar;
        _gameboardObjects.UnitRemoved += RemoveHealthBar;

        _gameboardState = gameboardState;
    }

    private void AddHealthBar(Unit unit)
    {
        Assert.IsNotNull(unit);
        Assert.IsFalse(_healthbars.ContainsKey(unit));

        unit.Removed += RemoveHealthBar;

        var comp = UIUtility.Add<UIHealthbar>(gameObject.transform, _healthbarPrototype.gameObject);
        comp.Initialize(unit, _gameboardState);

        _healthbars.Add(unit, comp);
    }

    private void RemoveHealthBar(Unit unit)
    {
        Assert.IsTrue(_healthbars.ContainsKey(unit));

        Destroy(_healthbars[unit].gameObject);

        _healthbars.Remove(unit);
    }
}
