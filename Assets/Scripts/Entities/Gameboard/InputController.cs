using UnityEngine;
using Framework;
using System;

public interface IInputEvents
{
    event Action<Tile> SpawnDefaultUnit;
    event Action<Tile> Select;
    event Action<Tile> CommitCurrentAction;
    event Action SetCurrentActionToMove;
    event Action SetCurrentActionToAttack;
    event Action Undo;
    event Action Continue;
    event Action<Tile> PreviewAction;
    event Action<Tile> HoveredTileChanged;
}

public class InputController : MonoBehaviour, IInputEvents
{
    public event Action<Tile> SpawnDefaultUnit;
    public event Action<Tile> Select;
    public event Action<Tile> CommitCurrentAction;
    public event Action SetCurrentActionToMove;
    public event Action SetCurrentActionToAttack;
    public event Action Undo;
    public event Action Continue;
    public event Action<Tile> PreviewAction;
    public event Action<Tile> HoveredTileChanged;

    private const KeyCode MoveKey = KeyCode.Alpha1;
    private const KeyCode PrimaryAttackKey = KeyCode.Alpha2;
    private const KeyCode SpawnDefaultUnitKey = KeyCode.F;
    private const KeyCode ContinueKey = KeyCode.Space;
    private const KeyCode UndoKey = KeyCode.Z;
    private const KeyCode PreviewKey = KeyCode.P; // DEBUG

    private Tile _lastTileUnderMouse;

    private void LateUpdate()
    {
        var tileUnderMouse = GetTileUnderMouse();

        if (Input.GetKeyDown(MoveKey))
            SetCurrentActionToMove.InvokeSafe();

        if (Input.GetKeyDown(PrimaryAttackKey))
            SetCurrentActionToAttack.InvokeSafe();

        if (Input.GetKeyDown(ContinueKey))
            Continue.InvokeSafe();

        if (Input.GetKeyDown(UndoKey))
            Undo.InvokeSafe();        

        if (tileUnderMouse != null)
        {
            if (Input.GetKeyDown(SpawnDefaultUnitKey))
                SpawnDefaultUnit.InvokeSafe(tileUnderMouse);

            if (Input.GetKeyDown(PreviewKey))
                PreviewAction?.Invoke(tileUnderMouse);

            if (Input.GetMouseButtonDown(0))
                Select.InvokeSafe(tileUnderMouse);

            if (Input.GetMouseButtonDown(1))
                CommitCurrentAction.InvokeSafe(tileUnderMouse);
        }

        if (tileUnderMouse != _lastTileUnderMouse)
            HoveredTileChanged?.Invoke(tileUnderMouse);

        _lastTileUnderMouse = tileUnderMouse;
    }

    public void TriggerSpawnDefaultUnit(Tile tile) => SpawnDefaultUnit?.Invoke(tile);
    public void TriggerSelect(Tile tile) => Select?.Invoke(tile);
    public void TriggerCommitCurrentAction(Tile tile) => CommitCurrentAction?.Invoke(tile);
    public void TriggerSetCurrentActionToMove() => SetCurrentActionToMove?.Invoke();
    public void TriggerSetCurrentActionToAttack() => SetCurrentActionToAttack?.Invoke();
    public void TriggerUndo() => Undo?.Invoke();
    public void TriggerContinue() => Continue?.Invoke();

    private T GetComponentUnderMouse<T>() where T : MonoBehaviour
    {
        var mouseToScreen = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        var rayResult = new RaycastHit();
        Physics.Raycast(new Ray(mouseToScreen, Camera.main.transform.forward), out rayResult, Mathf.Infinity);

        return rayResult.collider != null ? rayResult.collider.GetComponent<T>() : null;
    }

    private Tile GetTileUnderMouse()
    {
        return GetComponentUnderMouse<Tile>();
    }

    private Unit GetUnitUnderMouse()
    {
        var tile = GetTileUnderMouse();
        return tile != null && tile.Occupant != null ? tile.Occupant : null;
    }
}
