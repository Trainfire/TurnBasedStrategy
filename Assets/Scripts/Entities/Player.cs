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
    public PlayerInput Input { get; private set; }
    public Gameboard Gameboard { get; private set; }

    [SerializeField] private MechData _defaultMech;

    public void Initialize(Gameboard gameboard)
    {
        Gameboard = gameboard;

        Input = gameObject.GetOrAddComponent<PlayerInput>();
        Input.SpawnDefaultUnit += PlayerInput_SpawnDefaultUnit;
        Input.Select += PlayerInput_Select;
        Input.Undo += PlayerInput_Undo;

        MechHandler = new PlayerMechHandler(this);
        MechHandler.ActionCommitted += MechHandler_ActionCommitted;
    }

    private void PlayerInput_SpawnDefaultUnit(Tile tile)
    {
        if (tile != null &&  Gameboard != null)
        {
            if (_defaultMech == null)
            {
                DebugEx.LogWarning<Player>("Cannot spawn unit as no default unit is assigned.");
            }
            else
            {
                Gameboard.Objects.Spawn(tile, _defaultMech);
            }
        }
    }

    private void PlayerInput_Select(Tile tile)
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

    private void PlayerInput_Undo()
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
        Gameboard.Visualizer.Clear();
        MechHandler.Clear();
    }
}