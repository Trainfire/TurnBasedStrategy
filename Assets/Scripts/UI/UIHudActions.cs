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

    private State _state;
    private InputController _inputController;

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

    public void Initialize(Gameboard gameboard, InputController inputController)
    {
        _state = gameboard.State;
        _inputController = inputController;

        _buttonContinue = AddButton("Continue", () => inputController.TriggerContinue());
        _buttonUndo = AddButton("Undo", () => inputController.TriggerUndo());
        _buttonMove = AddButton("Move", () => inputController.TriggerSetCurrentActionToMove());
        _buttonAttack = AddButton("Attack", () => inputController.TriggerSetCurrentActionToAttack());
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
        if (_state == null)
            return;

        _buttonContinue.interactable = _state.Flags.CanContinue;
        _buttonUndo.gameObject.SetActive(_state.Flags.CanUndo);
        _buttonMove.gameObject.SetActive(_state.Flags.CanSelectedUnitMove);
        _buttonAttack.gameObject.SetActive(_state.Flags.CanSelectedUnitAttack);
    }
}
