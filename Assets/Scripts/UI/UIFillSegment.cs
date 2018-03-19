using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFillSegment : MonoBehaviour
{
    public bool Filled { get; set; }

    [SerializeField] private GameObject _filled;
    [SerializeField] private GameObject _empty;

    private void Update()
    {
        _filled.SetActive(Filled);
        _empty.SetActive(!Filled);
    }
}
