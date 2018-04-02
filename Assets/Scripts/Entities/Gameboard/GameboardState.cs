using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;
using Framework;

public enum GameboardStateID
{
    Invalid,
    Setup,
    PlayerMove,
    EnemyMove,
    GameOver,
}

public class GameboardStateController
{
    public const int MaxTurns = 5; // TODO: Define via data.

    public event Action GameWon;
    public event Action GameOver;

    public GameboardStateID Current { get; private set; }
    public int TurnCount { get; private set; }

    private Gameboard _gameboard;
    private Dictionary<GameboardStateID, GameboardState> _states;

    public GameboardStateController(Gameboard gameboard)
    {
        _gameboard = gameboard;

        _states = new Dictionary<GameboardStateID, GameboardState>();

        Register(new GameboardStateSetupPhase(_gameboard));
        Register(new GameboardStatePlayerMovePhase(_gameboard));

        Current = GameboardStateID.Setup;

        _states[Current].Enter();
    }

    private void Register(GameboardState gameboardState)
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

        if (previousStateID == GameboardStateID.EnemyMove)
        {
            Current = GameboardStateID.PlayerMove;
        }
        else
        {
            Current = GameboardStateID.EnemyMove;
        }

        TurnCount++;

        _states[Current].Enter();
    }

    private bool IsGameOver()
    {
        return false;
    }
}

public abstract class GameboardState
{
    public event Action<GameboardStateID> Exited;

    public abstract GameboardStateID StateID { get; }

    protected Gameboard Gameboard { get; private set; }

    public GameboardState(Gameboard gameboard)
    {
        Gameboard = gameboard;
    }

    public void Enter()
    {
        Debug.LogFormat("Enter state: [{0}]", GetType().Name);
        OnEnter();
    }

    protected virtual void OnEnter() { }

    protected void ExitState()
    {
        Debug.LogFormat("Exit state: [{0}]", GetType().Name);
        Exited.InvokeSafe(StateID);
    }
}

public class GameboardStateSetupPhase : GameboardState
{
    public override GameboardStateID StateID { get { return GameboardStateID.Setup; } }

    public GameboardStateSetupPhase(Gameboard gameboard) : base(gameboard) { }
}

public class GameboardStatePlayerMovePhase : GameboardState
{
    public override GameboardStateID StateID { get { return GameboardStateID.PlayerMove; } }

    public GameboardStatePlayerMovePhase(Gameboard gameboard) : base(gameboard) { }
}

public class GameboardStateEnemyMovePhase : GameboardState
{
    public override GameboardStateID StateID { get { return GameboardStateID.EnemyMove; } }

    public GameboardStateEnemyMovePhase(Gameboard gameboard) : base(gameboard) { }
}