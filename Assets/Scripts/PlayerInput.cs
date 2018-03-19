using UnityEngine;
using Framework;

public class PlayerInput : MonoBehaviour
{
    private Player _player;

    private void Awake()
    {
        _player = gameObject.GetComponentAssert<Player>();
    }

    private void LateUpdate()
    {
        var tileUnderMouse = GetTileUnderMouse();

        if (Input.GetKeyDown(KeyCode.F))
            _player.SpawnDefaultUnit(tileUnderMouse);

        if (Input.GetMouseButtonDown(0))
            _player.Select(tileUnderMouse);

        if (Input.GetMouseButtonDown(1))
            _player.CommitCurrentAction(tileUnderMouse);
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Label(_player.Selection != null ? _player.Selection.name : "Nobody");
        GUILayout.Label(_player.CurrentAction.ToString());
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Move"))
            _player.SetCurrentAction(UnitActionType.Move);

        if (GUILayout.Button("AttacK"))
            _player.SetCurrentAction(UnitActionType.AttackPrimary);

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
