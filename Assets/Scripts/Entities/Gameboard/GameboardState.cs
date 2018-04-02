using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Framework;

public enum GameboardStateID
{
    Invalid,
    Setup,
    PlayerMove,
    EnemyMove,
    GameOver,
}

public class ValidPlayerActions
{
    public bool CanContinue { get; set; }
    public bool CanControlUnits { get; set; }
    public bool CanUndo { get; set; }

    public void Clear()
    {
        CanContinue = false;
        CanControlUnits = false;
        CanUndo = false;
    }

    public ReadOnlyValidPlayerActions AsReadOnly()
    {
        return new ReadOnlyValidPlayerActions(this);
    }
}

public struct ReadOnlyValidPlayerActions
{
    public readonly bool CanContinue;
    public readonly bool CanControlUnits;
    public readonly bool CanUndo;

    public ReadOnlyValidPlayerActions(ValidPlayerActions validPlayerActions)
    {
        CanContinue = validPlayerActions.CanContinue;
        CanControlUnits = validPlayerActions.CanControlUnits;
        CanUndo = validPlayerActions.CanUndo;
    }
}


public class GameboardState
{
    public const int MaxTurns = 5; // TODO: Define via data.

    public event Action GameWon;
    public event Action GameOver;

    public GameboardStateID Current { get; private set; }
    public ReadOnlyValidPlayerActions ValidPlayerActions { get { return _states[Current].ValidPlayerActions.AsReadOnly(); } }
    public int TurnCount { get; private set; }

    private Dictionary<GameboardStateID, GameboardStateBase> _states;

    public GameboardState(GameboardObjects gameboardObjects, GameboardInput playerInput)
    {
        _states = new Dictionary<GameboardStateID, GameboardStateBase>();

        Register(new GameboardStateSetupPhase(gameboardObjects, playerInput));
        Register(new GameboardStatePlayerMovePhase(playerInput));

        Current = GameboardStateID.Setup;

        _states[Current].Enter();
    }

    private void Register(GameboardStateBase gameboardState)
    {
        Assert.IsFalse(_states.ContainsKey(gameboardState.StateID));

        _states.Add(gameboardState.StateID, gameboardState);

        gameboardState.Exited += MoveNext;
    }

    private void MoveNext(GameboardStateID previousStateID)
    {
        if (TurnCount == MaxTurns)
        {
            GameWon.InvokeSafe();
            return;
        }

        if (IsGameOver())
        {
            GameOver.InvokeSafe();
            return;
        }

        // TODO: Add logic for moving to other states.
        if (previousStateID == GameboardStateID.Setup)
            Current = GameboardStateID.PlayerMove;

        TurnCount++;

        Assert.IsTrue(_states.ContainsKey(Current), "No handler found for state.");

        _states[Current].Enter();
    }

    private bool IsGameOver()
    {
        return false;
    }
}

public abstract class GameboardStateBase
{
    public event Action<GameboardStateID> Exited;

    public abstract GameboardStateID StateID { get; }

    public virtual bool CanExitState { get { return true; } }
    public virtual ValidPlayerActions ValidPlayerActions { get; private set; }

    public void Enter()
    {
        ValidPlayerActions = new ValidPlayerActions();

        Debug.LogFormat("Enter state: [{0}]", StateID);
        OnEnter();
    }

    protected virtual void OnEnter() { }

    protected void ExitState()
    {
        Debug.LogFormat("Exit state: [{0}]", StateID);
        ValidPlayerActions.Clear();
        Exited.InvokeSafe(StateID);
    }
}

public class GameboardStateSetupPhase : GameboardStateBase
{
    public override GameboardStateID StateID { get { return GameboardStateID.Setup; } }

    private GameboardObjects _gameboardObjects;
    private GameboardInput _playerInput;

    public GameboardStateSetupPhase(GameboardObjects gameboardObjects, GameboardInput playerInput)
    {
        _gameboardObjects = gameboardObjects;
        _playerInput = playerInput;
    }

    protected override void OnEnter()
    {
        _gameboardObjects.UnitAdded += OnUnitAdded;
        _playerInput.Continue += OnPlayerInputContinue;
    }

    private void OnUnitAdded(Unit unit)
    {
        ValidPlayerActions.CanContinue = _gameboardObjects.Mechs.Count == 3;
    }

    private void OnPlayerInputContinue()
    {
        if (ValidPlayerActions.CanContinue)
        {
            _gameboardObjects.UnitAdded -= OnUnitAdded;
            _playerInput.Continue -= OnPlayerInputContinue;
            ExitState();
        }
    }
}

public class GameboardStatePlayerMovePhase : GameboardStateBase
{
    public override GameboardStateID StateID { get { return GameboardStateID.PlayerMove; } }

    private GameboardInput _playerInput;

    public GameboardStatePlayerMovePhase(GameboardInput playerInput)
    {
        _playerInput = playerInput;
    }

    protected override void OnEnter()
    {
        _playerInput.Undo += OnPlayerInputUndo;
        _playerInput.Continue += OnPlayerInputContinue;

        ValidPlayerActions.CanControlUnits = true;
    }

    private void OnPlayerInputContinue()
    {
        _playerInput.Undo -= OnPlayerInputUndo;
        _playerInput.Continue -= OnPlayerInputContinue;

        ExitState();
    }

    private void OnPlayerInputUndo()
    {
        // TODO.
    }
}