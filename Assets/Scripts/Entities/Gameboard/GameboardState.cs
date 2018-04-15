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

public class GameboardStateFlags
{
    public bool CanContinue { get; set; }
    public bool CanControlUnits { get; set; }
    public bool CanSpawnUnits { get; set; }
    public bool CanUndo { get; set; }

    public void Clear()
    {
        CanContinue = false;
        CanControlUnits = false;
        CanSpawnUnits = false;
        CanUndo = false;
    }

    public ReadOnlyGameboardStateFlags AsReadOnly()
    {
        return new ReadOnlyGameboardStateFlags(this);
    }
}

public struct ReadOnlyGameboardStateFlags
{
    public readonly bool CanContinue;
    public readonly bool CanControlUnits;
    public readonly bool CanSpawnUnits;
    public readonly bool CanUndo;

    public ReadOnlyGameboardStateFlags(GameboardStateFlags validPlayerActions)
    {
        CanContinue = validPlayerActions.CanContinue;
        CanControlUnits = validPlayerActions.CanControlUnits;
        CanSpawnUnits = validPlayerActions.CanSpawnUnits;
        CanUndo = validPlayerActions.CanUndo;
    }
}


public class GameboardState
{
    public const int MaxTurns = 5; // TODO: Define via data.

    public event Action EffectPreviewEntered;
    public event Action EffectPreviewLeft;
    public event Action<EffectPreview> EffectPreviewChanged;

    public event Action GameWon;
    public event Action GameOver;

    public Tile CurrentSelection { get; private set; }

    public GameboardStateID Current { get; private set; }
    public ReadOnlyGameboardStateFlags Flags { get { return _states[Current].Flags.AsReadOnly(); } }
    public int TurnCount { get; private set; }

    private Dictionary<GameboardStateID, GameboardStateBase> _states;

    private GameboardObjects _gameboardObjects;

    public GameboardState(Gameboard gameboard)
    {
        _gameboardObjects = gameboard.Objects;

        _states = new Dictionary<GameboardStateID, GameboardStateBase>();

        gameboard.InputEvents.Select += OnPlayerInputSelect;

        Register(new GameboardStateSetupPhase(gameboard.Objects, gameboard.InputEvents));
        Register(new GameboardStatePlayerMovePhase(this, gameboard));
        Register(new GameboardStateGameOver());

        foreach (var unit in gameboard.Objects.Units.ToList())
        {
            if (unit.GetType() == typeof(Building))
                unit.Health.Changed += OnBuildingHealthChanged;
        }

        Current = GameboardStateID.Setup;

        _states[Current].Enter();
    }

    private void Register(GameboardStateBase gameboardState)
    {
        Assert.IsFalse(_states.ContainsKey(gameboardState.StateID));

        _states.Add(gameboardState.StateID, gameboardState);

        gameboardState.Exited += MoveNext;
        gameboardState.EffectPreviewed += OnGameboardStateEffectPreviewed;
    }

    private void MoveNext(GameboardStateID previousStateID)
    {
        if (TurnCount == MaxTurns)
        {
            GameWon.InvokeSafe();
            return;
        }

        // TODO: Add logic for moving to other states.
        if (previousStateID == GameboardStateID.Setup)
            Current = GameboardStateID.PlayerMove;

        TurnCount++;

        Assert.IsTrue(_states.ContainsKey(Current), "No handler found for state.");

        _states[Current].Enter();
    }

    private void OnPlayerInputSelect(Tile selection)
    {
        if (selection == null)
            return;

        CurrentSelection = selection;

        DebugEx.Log<GameboardState>("Selected tile: " + CurrentSelection.name);
    }

    private void OnBuildingHealthChanged(HealthChangeEvent healthChangeEvent)
    {
        if (_gameboardObjects.Buildings.All(x => x.Health.Current == 0))
        {
            Current = GameboardStateID.GameOver;
            _states[Current].Enter();
            GameOver.InvokeSafe();
        }
    }

    private void OnGameboardStateEffectPreviewed(EffectPreview effectPreview) => EffectPreviewChanged?.Invoke(effectPreview);
}

public abstract class GameboardStateBase
{
    public event Action<EffectPreview> EffectPreviewed;
    public event Action<GameboardStateID> Exited;

    public abstract GameboardStateID StateID { get; }

    public virtual bool CanExitState { get { return true; } }
    public virtual GameboardStateFlags Flags { get; private set; }

    public void Enter()
    {
        Flags = new GameboardStateFlags();

        Debug.LogFormat("Enter state: [{0}]", StateID);
        OnEnter();
    }

    protected virtual void OnEnter() { }

    protected void ExitState()
    {
        Debug.LogFormat("Exit state: [{0}]", StateID);
        Flags.Clear();
        Exited.InvokeSafe(StateID);
    }

    protected void PreviewEffect(EffectPreview effectPreview) => EffectPreviewed?.Invoke(effectPreview);
}