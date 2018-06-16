using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Linq;

public class UIFillBar : MonoBehaviour
{
    [SerializeField] private Transform _fillParent;
    [SerializeField] private UIFillSegment _fillSegmentPrototype;
    [SerializeField] private float _offset;

    private List<UIFillSegment> _fillSegmentInstances = new List<UIFillSegment>();

    public int FilledCount
    {
        set
        {
            for (int i = 0; i < _fillSegmentInstances.Count; i++)
            {
                _fillSegmentInstances[i].Filled = i < Mathf.Min(value, _fillSegmentInstances.Count);
            }
        }
    }

    public int SegmentCount
    {
        set
        {
            _fillSegmentInstances.ForEach(x => Destroy(x.gameObject));
            _fillSegmentInstances.Clear();

            for (int i = 0; i < Mathf.Max(value, 0); i++)
            {
                var segmentInstance = GameObject.Instantiate(_fillSegmentPrototype, _fillParent);
                segmentInstance.gameObject.SetActive(true);
                segmentInstance.Filled = true;

                _fillSegmentInstances.Add(segmentInstance);
            }
        }
    }

    private void Awake()
    {
        Assert.IsNotNull(_fillSegmentPrototype);
        _fillSegmentPrototype.gameObject.SetActive(false);
    }

    public void PreviewFilledSegmentCount(int newCount)
    {
        // Set healthbars that will be removed so that they flash.
        for (int i = 0; i < _fillSegmentInstances.Count; i++)
        {
            if (i < _fillSegmentInstances.Count - Mathf.Abs(newCount))
            {
                _fillSegmentInstances[i].Flashing = false;
            }
            else
            {
                _fillSegmentInstances[i].Flashing = true;
            }
        }
    }
}