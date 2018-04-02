using UnityEngine;
using UnityEngine.Assertions;
using Framework;
using System;

public enum UnitActionType
{
    Unassigned,
    Move,
    AttackPrimary,
    AttackSecondary,
}

public struct UnitAction
{
    public Unit Actor;
    public UnitActionType Action;
}

public class Player : MonoBehaviour
{
    public Unit Selection { get; private set; }
    public PlayerMechHandler MechHandler { get; private set; }

    [SerializeField] private MechData _defaultMech;

    private Gameboard _gameboard;

    public void Initialize(Gameboard gameboard)
    {
        _gameboard = gameboard;
        _gameboard.Input.SpawnDefaultUnit += OnInputSpawnDefaultUnit;
        _gameboard.Input.Select += OnInputSelect;
        _gameboard.Input.Undo += OnInputUndo;
        _gameboard.Input.SetCurrentActionToMove += OnInputSetCurrentActionToMove;
        _gameboard.Input.SetCurrentActionToAttack += OnInputSetCurrentActionToAttack;
        _gameboard.Input.CommitCurrentAction += OnInputCommitCurrentAction;

        MechHandler = new PlayerMechHandler(_gameboard.Helper, _gameboard.Visualizer, _gameboard.State);
        MechHandler.ActionCommitted += MechHandler_ActionCommitted;
    }

    private void OnInputCommitCurrentAction(Tile tile)
    {
        MechHandler.CommitCurrentAction(tile);
    }

    private void OnInputSetCurrentActionToAttack()
    {
        MechHandler.SetCurrentAction(UnitActionType.AttackPrimary);
    }

    private void OnInputSetCurrentActionToMove()
    {
        MechHandler.SetCurrentAction(UnitActionType.Move);
    }

    private void OnInputSpawnDefaultUnit(Tile tile)
    {
        if (tile != null &&  _gameboard != null)
        {
            if (_defaultMech == null)
            {
                DebugEx.LogWarning<Player>("Cannot spawn unit as no default unit is assigned.");
            }
            else
            {
                _gameboard.Objects.Spawn(tile, _defaultMech);
            }
        }
    }

    private void OnInputSelect(Tile tile)
    {
        if (tile == null)
            return;

        if (tile.Occupant != null)
        {
            Selection = tile.Occupant;

            var selectionAsMech = Selection as Mech;
            if (selectionAsMech != null)
                MechHandler.Set(selectionAsMech);
        }
        else
        {
            ClearSelection();
        }
    }

    private void OnInputUndo()
    {
        DebugEx.Log<Player>("Not implemented: Undo");
    }

    private void MechHandler_ActionCommitted(PlayerMechHandlerActionCommittedEvent playerMechHandlerActionCommittedEvent)
    {
        ClearSelection();
    }

    private void ClearSelection()
    {
        Selection = null;
        _gameboard.Visualizer.Clear();
        MechHandler.Clear();
    }
}