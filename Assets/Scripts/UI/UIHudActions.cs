using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;
using Framework;
using Framework.UI;

public class UIHudActions : MonoBehaviour
{
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private Button _prototype;
    private List<Button> _instances;

    private GameboardState _gameboardState;
    private GameboardInput _gameboardInput;
    private Button _buttonContinue;
    private Button _buttonUndo;
    private Button _buttonMove;
    private Button _buttonAttack;

    private void Awake()
    {
        Assert.IsNotNull(_prototype);
        _prototype.gameObject.SetActive(false);

        _instances = new List<Button>();
    }

    public void Initialize(GameboardState gameboardState, GameboardInput gameboardInput)
    {
        _gameboardState = gameboardState;
        _gameboardInput = gameboardInput;

        _buttonContinue = AddButton("Continue", () => gameboardInput.TriggerContinue());
        _buttonUndo = AddButton("Undo", () => gameboardInput.TriggerUndo());
        _buttonMove = AddButton("Move", () => gameboardInput.TriggerSetCurrentActionToMove());
        _buttonAttack = AddButton("Attack", () => gameboardInput.TriggerSetCurrentActionToAttack());
    }

    private Button AddButton(string label, Action onClick)
    {
        Assert.IsNotNull(_prototype);

        var instance = UIUtility.Add<Button>(_targetTransform, _prototype.gameObject);
        instance.onClick.AddListener(() => onClick());

        var textComp = instance.GetComponentInChildren<Text>();
        if (textComp != null)
            textComp.text = label;

        _instances.Add(instance);

        return instance;
    }

    private void Update()
    {
        if (_gameboardState == null)
            return;

        _buttonContinue.interactable = _gameboardState.Flags.CanContinue;
        _buttonUndo.gameObject.SetActive(_gameboardState.Flags.CanUndo);
        _buttonMove.gameObject.SetActive(_gameboardState.Flags.CanControlUnits);
        _buttonAttack.gameObject.SetActive(_gameboardState.Flags.CanControlUnits);
    }
}
