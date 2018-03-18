using Framework;
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
        Reachable,
    };

    public DebugType Type;
    public int Distance;
    public Vector2 GridPosition;
    public Vector2 TargetGridPosition;
    public GameboardDirection Direction;

    private Gameboard _gameboard;
    private GameboardHelper _gameboardHelper;
    private List<TileResult> _tileResults;

    private void Awake()
    {
        _gameboard = gameObject.GetComponentAssert<Gameboard>();
        _gameboardHelper = new GameboardHelper(_gameboard);
        _tileResults = new List<TileResult>();
    }

    private void FixedUpdate()
    {
        UpdateTiles();
    }

    private void UpdateTiles()
    {
        var originTile = _gameboardHelper.GetTile(GridPosition);

        if (originTile != null)
        {
            switch (Type)
            {
                case DebugType.Radius: _tileResults = _gameboardHelper.GetTilesInRadius(originTile, Distance); break;
                case DebugType.Reachable: _tileResults = _gameboardHelper.GetReachableTiles(originTile, Distance); break;
                case DebugType.Line: _tileResults = _gameboardHelper.GetTiles(originTile, Direction, Distance); break;
            }
        }
    }

    private void OnGUI()
    {
        foreach (var result in _tileResults)
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
