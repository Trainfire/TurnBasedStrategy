using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Gameboard))]
public class GameboardDebugger : MonoBehaviour
{
    public enum DebugType
    {
        Radius,
        Line,
        Reachable,
    };

    public DebugType Type;
    public int Distance;
    public Vector2 GridPosition;
    public Vector2 TargetGridPosition;
    public GameboardDirection Direction;

    private Gameboard myGameboard;
    private List<TileResult> myTileResults;

    private void Awake()
    {
        myGameboard = GetComponent<Gameboard>();
        Assert.IsNotNull(myGameboard);
    }

    private void Start()
    {
        UpdateTiles();
    }

    private void FixedUpdate()
    {
        UpdateTiles();
    }

    private void UpdateTiles()
    {
        var originTile = myGameboard.GetTile(GridPosition);

        if (originTile != null)
        {
            switch (Type)
            {
                case DebugType.Radius: myTileResults = myGameboard.GetTilesInRadius(originTile, Distance); break;
                case DebugType.Reachable: myTileResults = myGameboard.GetReachableTiles(originTile, Distance); break;
                case DebugType.Line: myTileResults = myGameboard.GetTiles(originTile, Direction, Distance); break;
            }
        }
    }

    private void OnGUI()
    {
        foreach (var result in myTileResults)
        {
            if (result.Tile.Occupied)
            {
                result.Tile.Marker.SetNegative();
            }
            else
            {
                result.Tile.Marker.SetPositive();
            }

            result.Tile.Marker.DrawText(result.Distance.ToString());
        }
    }
}
