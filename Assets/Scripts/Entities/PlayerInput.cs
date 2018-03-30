using UnityEngine;
using Framework;
using System;

public class PlayerInput : MonoBehaviour
{
    public event Action<Tile> SpawnDefaultUnit;
    public event Action<Tile> Select;
    public event Action<Tile> CommitCurrentAction;
    public event Action<UnitActionType> SetCurrentAction;
    public event Action Undo;

    private Player _player;

    private void Awake()
    {
        _player = gameObject.GetComponentAssert<Player>();
    }

    private void LateUpdate()
    {
        var tileUnderMouse = GetTileUnderMouse();

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetCurrentAction.InvokeSafe(UnitActionType.Move);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SetCurrentAction.InvokeSafe(UnitActionType.AttackPrimary);

        if (Input.GetKeyDown(KeyCode.F))
            SpawnDefaultUnit.InvokeSafe(tileUnderMouse);

        if (Input.GetMouseButtonDown(0))
            Select.InvokeSafe(tileUnderMouse);

        if (Input.GetMouseButtonDown(1))
            CommitCurrentAction.InvokeSafe(tileUnderMouse);
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Label(_player.Selection != null ? _player.Selection.name : "N/A");
        GUILayout.Label(_player.MechHandler.CurrentAction.ToString());
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Move"))
            SetCurrentAction.InvokeSafe(UnitActionType.Move);

        if (GUILayout.Button("AttacK"))
            SetCurrentAction.InvokeSafe(UnitActionType.AttackPrimary);

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
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
