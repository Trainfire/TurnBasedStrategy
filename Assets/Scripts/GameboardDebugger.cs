using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Gameboard))]
public class GameboardDebugger : MonoBehaviour
{
    public enum DebugType
    {
        Radius,
        Line,
    };

    public DebugType Type;
    public int Distance;
    public Vector2 GridPosition;
    public GameboardDirection Direction;

    private Gameboard myGameboard;

    private void Awake()
    {
        myGameboard = GetComponent<Gameboard>();
        Assert.IsNotNull(myGameboard);
    }

    private void OnGUI()
    {
        var originTile = myGameboard.GetTile(GridPosition);

        if (originTile != null)
        {
            var results = new List<TileResult>();

            switch (Type)
            {
                case DebugType.Radius: results = myGameboard.GetValidMovementTiles(originTile, Distance); break;
                case DebugType.Line: results = myGameboard.GetTiles(originTile, Direction, Distance); break;
            }

            results.ForEach(x => x.Tile.Marker.SetPositive());

            foreach (var result in results)
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
}
