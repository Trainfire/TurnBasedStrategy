using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class UIGlobalHealth : MonoBehaviour
{
    [SerializeField] private UIFillBar _fillbar;

    private IStateData _stateData;

    private void Awake()
    {
        Assert.IsNotNull(_fillbar);
    }

    public void Initialize(IStateData stateData)
    {
        _stateData = stateData;

        _fillbar.SegmentCount = stateData.GlobalMaxHealth;
        _fillbar.FilledCount = stateData.GlobalHealth;
    }

    public void Update()
    {
        if (_stateData == null)
            return;

        _fillbar.FilledCount = _stateData.GlobalHealth;
    }
}
