using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class UITurnCount : MonoBehaviour
{
    [SerializeField] private Text _text;
    private Gameboard _gameboard;

    private void Awake()
    {
        Assert.IsNotNull(_text);
    }

    public void Initialize(Gameboard gameboard)
    {
        _gameboard = gameboard;
    }

    public void Update()
    {
        if (_gameboard != null)
            _text.text = $"Turns Left: {_gameboard.State.Sequencer.TurnsLeft.ToString()}";
    }
}
