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
    public UnitActionType CurrentAction { get; private set; }

    [SerializeField] private UnitData _defaultUnit;

    private Gameboard _gameboard;
    private PlayerInput _playerInput;

    void Awake()
    {
        _gameboard = FindObjectOfType<Gameboard>();
        Assert.IsNotNull(_gameboard);

        _playerInput = gameObject.GetOrAddComponent<PlayerInput>();
        _playerInput.SpawnDefaultUnit += PlayerInput_SpawnDefaultUnit;
        _playerInput.Select += PlayerInput_Select;
        _playerInput.CommitCurrentAction += PlayerInput_CommitCurrentAction;
        _playerInput.SetCurrentAction += PlayerInput_SetCurrentAction;
        _playerInput.Undo += PlayerInput_Undo;
    }

    private void PlayerInput_SpawnDefaultUnit(Tile tile)
    {
        if (tile != null &&  _gameboard != null)
        {
            if (_defaultUnit == null)
            {
                DebugEx.LogWarning<Player>("Cannot spawn unit as no default unit is assigned.");
            }
            else
            {
                _gameboard.SpawnUnit(new SpawnAction(_defaultUnit, tile));
            }
        }
    }

    private void PlayerInput_SetCurrentAction(UnitActionType actionType)
    {
        if (Selection == null)
            return;

        CurrentAction = actionType;

        if (CurrentAction == UnitActionType.Move)
            _gameboard.Visualizer.ShowReachablePositions(Selection);

        if (CurrentAction == UnitActionType.AttackPrimary)
            _gameboard.Visualizer.ShowTargetableTiles(Selection, Selection.PrimaryWeapon.Data);
    }

    private void PlayerInput_CommitCurrentAction(Tile targetTile)
    {
        if (Selection == null || CurrentAction == UnitActionType.Unassigned || targetTile == null)
            return;

        var actionComplete = false;
        if(CurrentAction == UnitActionType.Move)
            actionComplete = Selection.MoveTo(targetTile);

        if (CurrentAction == UnitActionType.AttackPrimary)
        {
            // Temp
            Selection.Attack(targetTile);
            actionComplete = true;
        }

        if (actionComplete)
        {
            DebugEx.Log<Player>("Commit action '{0}' on tile {1} using unit {2}", CurrentAction, targetTile.Position, Selection.name);
            ClearSelection();
        }
    }

    private void PlayerInput_Select(Tile tile)
    {
        if (tile == null)
            return;

        if (tile.Occupied)
        {
            Selection = tile.Occupant;
        }
        else
        {
            ClearSelection();
        }
    }

    private void PlayerInput_Undo()
    {
        DebugEx.Log<Player>("Not implemented: Undo");
    }

    private void ClearSelection()
    {
        Selection = null;
        CurrentAction = UnitActionType.Unassigned;
        _gameboard.Visualizer.Clear();
    }
}