using UnityEngine;
using Framework;
using System;

public class GameboardInput : MonoBehaviour
{
    public event Action<Tile> SpawnDefaultUnit;
    public event Action<Tile> Select;
    public event Action<Tile> CommitCurrentAction;
    public event Action SetCurrentActionToMove;
    public event Action SetCurrentActionToAttack;
    public event Action Undo;
    public event Action Continue;

    private const KeyCode MoveKey = KeyCode.Alpha1;
    private const KeyCode PrimaryAttackKey = KeyCode.Alpha2;
    private const KeyCode SpawnDefaultUnitKey = KeyCode.F;
    private const KeyCode ContinueKey = KeyCode.Space;
    private const KeyCode UndoKey = KeyCode.Z;

    private void LateUpdate()
    {
        var tileUnderMouse = GetTileUnderMouse();

        if (Input.GetKeyDown(MoveKey))
            SetCurrentActionToMove.InvokeSafe();

        if (Input.GetKeyDown(PrimaryAttackKey))
            SetCurrentActionToAttack.InvokeSafe();

        if (Input.GetKeyDown(SpawnDefaultUnitKey))
            SpawnDefaultUnit.InvokeSafe(tileUnderMouse);

        if (Input.GetKeyDown(ContinueKey))
            Continue.InvokeSafe();

        if (Input.GetMouseButtonDown(0))
            Select.InvokeSafe(tileUnderMouse);

        if (Input.GetMouseButtonDown(1))
            CommitCurrentAction.InvokeSafe(tileUnderMouse);

        if (Input.GetKeyDown(UndoKey))
            Undo.InvokeSafe();
    }

    private void OnGUI()
    {
        // Begin temp garbage.

        GUILayout.BeginVertical();

        GUILayout.Label("Move: " + MoveKey.ToString());
        GUILayout.Label("Attack: " + PrimaryAttackKey.ToString());
        GUILayout.Label("Continue: " + ContinueKey.ToString());
        GUILayout.Label("Undo: " + UndoKey.ToString());

        GUILayout.Label("Spawn: " + SpawnDefaultUnitKey.ToString());

        GUILayout.EndHorizontal();

        // End temp garbage.
    }

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
