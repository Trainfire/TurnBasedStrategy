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

    void Awake()
    {
        _gameboard = FindObjectOfType<Gameboard>();
        Assert.IsNotNull(_gameboard);
    }

    public void SpawnDefaultUnit(Tile tile)
    {
        if (tile != null && _defaultUnit != null && _gameboard != null)
            _gameboard.SpawnUnit(new SpawnAction(_defaultUnit, tile));
    }

    public void SetCurrentAction(UnitActionType actionType)
    {
        CurrentAction = actionType;
    }

    public void CommitCurrentAction(Tile targetTile)
    {
        if (Selection == null || CurrentAction == UnitActionType.Unassigned || targetTile == null)
            return;

        DebugEx.Log<Player>("Commit action '{0}' on tile {1} using unit {2}", CurrentAction, targetTile.Position, Selection.name);

        if(CurrentAction == UnitActionType.Move)
            Selection.Move(targetTile);
    }

    public void Select(Tile tile)
    {
        if (tile == null)
            return;

        if (tile.Occupied)
        {
            Selection = tile.Occupant;
        }
        else
        {
            Selection = null;
            CurrentAction = UnitActionType.Unassigned;
        }
    }

    public void Undo()
    {
        DebugEx.Log<Player>("Not implemented: Undo");
    }
}