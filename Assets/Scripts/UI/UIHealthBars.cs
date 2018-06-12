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

    private Events _events;

    private void Awake()
    {
        Assert.IsNotNull(_healthbarPrototype);
        _healthbarPrototype.gameObject.SetActive(false);
        _healthbars = new Dictionary<Unit, UIHealthbar>();
    }

    public void Initialize(Gameboard gameboard)
    {
        _events = gameboard.Events;
        _events.World.UnitAdded += AddHealthBar;

        // Add any existing units.
        gameboard.World.Units.ToList().ForEach(x => AddHealthBar(x));
    }

    private void AddHealthBar(Unit unit)
    {
        Assert.IsNotNull(unit);
        Assert.IsFalse(_healthbars.ContainsKey(unit));

        unit.Removed += RemoveHealthBar;

        var comp = UIUtility.Add<UIHealthbar>(gameObject.transform, _healthbarPrototype.gameObject);
        comp.Initialize(unit, _events.State);

        _healthbars.Add(unit, comp);
    }

    private void RemoveHealthBar(Unit unit)
    {
        Assert.IsTrue(_healthbars.ContainsKey(unit));

        unit.Removed -= RemoveHealthBar;

        Destroy(_healthbars[unit].gameObject);

        _healthbars.Remove(unit);
    }

    private void OnDestroy()
    {
        _healthbars.Keys.ToList().ForEach(x => x.Removed -= RemoveHealthBar);
        _healthbars.Clear();
    }
}
