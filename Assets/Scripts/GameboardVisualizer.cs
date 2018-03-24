using Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Gameboard))]
public class GameboardVisualizer : MonoBehaviour
{
    private Gameboard _gameboard;
    private List<TileResult> _tileResults;

    private void Awake()
    {
        _gameboard = gameObject.GetComponentAssert<Gameboard>();
        _tileResults = new List<TileResult>();
    }

    public void ShowReachablePositions(Unit unit)
    {
        _tileResults = _gameboard.Helper.GetReachableTiles(unit);
    }

    public void ShowTargetableTiles(Unit unit, WeaponData weaponData)
    {
        _tileResults = _gameboard.Helper.GetTargetableTiles(unit, weaponData);
    }

    public void Clear()
    {
        _tileResults.Clear();
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
