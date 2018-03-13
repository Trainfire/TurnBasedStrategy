﻿using UnityEngine;
using Framework;

public class Player : MonoBehaviour
{
    public Unit UnitPrototype; // Temp.

    private PlayerUnitPlacer myUnitPlacer;
    private Unit mySelectedUnit;
    private GameboardTile myTileUnderMouse;
    private bool myInputEnabled;

    private void Awake()
    {
        myUnitPlacer = gameObject.GetOrAddComponent<PlayerUnitPlacer>();
        myInputEnabled = true;
    }

    void Update()
    {
        myTileUnderMouse = GetTileUnderMouse();

        // Highlight tile under mouse
        if (myTileUnderMouse)
        {
            var color = myTileUnderMouse.Occupied ? Color.red : Color.green;
            Debug.DrawLine(myTileUnderMouse.transform.position, myTileUnderMouse.transform.position + (Vector3.up * 4f), color);
        }
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("Selected Unit: " + (mySelectedUnit != null ? mySelectedUnit.name : "Null"));
        GUILayout.EndVertical();
    }

    void LateUpdate()
    {
        if (!myInputEnabled)
            return;

        if (myUnitPlacer.IsPlacingUnit)
        {
            if (Input.GetMouseButtonUp(1))
                myUnitPlacer.CancelPlacingUnit();

            if (Input.GetMouseButtonDown(0))
            {
                var placementResult = myUnitPlacer.PlaceUnit(myTileUnderMouse);
                Debug.LogFormat("Place unit on tile {0}", placementResult.Tile.name);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.F))
                myUnitPlacer.BeginPlacingUnit(UnitPrototype);

            if (Input.GetMouseButtonDown(0))
            {
                var unitUnderMouse = GetUnitUnderMouse();

                // Deselect unit if unit is selected.
                if (mySelectedUnit != null && unitUnderMouse == null)
                {
                    mySelectedUnit = null;
                }
                else
                {
                    mySelectedUnit = unitUnderMouse;
                }
            }

            // Move.
            if (Input.GetMouseButtonDown(1) && myTileUnderMouse != null && mySelectedUnit != null)
                mySelectedUnit.Move(myTileUnderMouse);
        }
    }

    static T GetComponentUnderMouse<T>() where T : MonoBehaviour
    {
        var mouseToScreen = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        var rayResult = new RaycastHit();
        Physics.Raycast(new Ray(mouseToScreen, Camera.main.transform.forward), out rayResult, Mathf.Infinity);

        return rayResult.collider != null ? rayResult.collider.GetComponent<T>() : null;
    }

    public static GameboardTile GetTileUnderMouse()
    {
        return GetComponentUnderMouse<GameboardTile>();
    }

    public static Unit GetUnitUnderMouse()
    {
        var tile = GetTileUnderMouse();
        return tile != null && tile.Occupant != null ? tile.Occupant : null;
    }
}