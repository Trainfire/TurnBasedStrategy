using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Gameboard))]
public class GameboardMarkers : MonoBehaviour
{
    public bool AllDirections;
    public GameboardDirection Direction;
    public int Length;
    public bool FilterOccupiedTiles;
    public GameboardTile OriginTile; // Temp

    private Gameboard myGameboard;

    private void Awake()
    {
        myGameboard = GetComponent<Gameboard>();
        Assert.IsNotNull(myGameboard);
    }

    private void Update()
    {
        if (OriginTile != null)
        {
            var tiles = AllDirections ? myGameboard.GetTilesFromAllDirections(OriginTile, Length, FilterOccupiedTiles) : myGameboard.GetTiles(OriginTile, Direction, Length, FilterOccupiedTiles);
            foreach (var tile in tiles)
            {
                Debug.DrawLine(tile.transform.position, tile.transform.position + Vector3.up * 2f, Color.green);
            }
        }
    }
}
