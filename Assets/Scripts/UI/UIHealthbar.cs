using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Collections.Generic;

public class UIHealthbar : MonoBehaviour
{
    [SerializeField] private Transform _fillParent;
    [SerializeField] private UIFillSegment _fillSegmentPrototype;
    [SerializeField] private float _offset;

    private Unit _unit;
    private List<UIFillSegment> _fillSegmentInstances;

    private void Awake()
    {
        Assert.IsNotNull(_fillSegmentPrototype);
        _fillSegmentPrototype.gameObject.SetActive(false);

        _fillSegmentInstances = new List<UIFillSegment>();
    }

    public void Initialize(Unit unit)
    {
        Assert.IsNotNull(unit);

        _unit = unit;

        Assert.IsNotNull(_unit.Health);

        _unit.Health.Changed += OnUnitHealthChanged;

        for (int i = 0; i < _unit.Health.Max; i++)
        {
            var segmentInstance = GameObject.Instantiate(_fillSegmentPrototype, _fillParent);
            segmentInstance.gameObject.SetActive(true);
            segmentInstance.Filled = true;

            _fillSegmentInstances.Add(segmentInstance);
        }
    }

    private void Update()
    {
        if (_unit == null)
            return;

        var worldToScreen = Camera.main.WorldToScreenPoint(_unit.transform.position);
        transform.position = worldToScreen + (Vector3.up * _offset);
    }

    private void OnUnitHealthChanged(HealthChangeEvent healthChangeEvent)
    {
        for (int i = 0; i < _fillSegmentInstances.Count; i++)
        {
            _fillSegmentInstances[i].Filled = i < healthChangeEvent.Health.Current;
        }
    }
}