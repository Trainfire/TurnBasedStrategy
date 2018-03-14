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
    public Vector2 OriginTile; // Temp
    public bool ActualDistances;

    private Gameboard myGameboard;

    private void Awake()
    {
        myGameboard = GetComponent<Gameboard>();
        Assert.IsNotNull(myGameboard);
    }

    private void OnGUI()
    {
        var originTile = myGameboard.GetTile(OriginTile);

        if (originTile != null)
        {
            //var tiles = AllDirections ? myGameboard.GetTilesFromAllDirections(OriginTile, Length, FilterOccupiedTiles) : myGameboard.GetTiles(OriginTile, Direction, Length, FilterOccupiedTiles);
            var results = myGameboard.GetValidMovementTiles(originTile, Length);
            foreach (var result in results)
            {
                Debug.DrawLine(result.Tile.transform.position, result.Tile.transform.position + Vector3.up * 2f, Color.green);

                var worldToScreen = Camera.main.WorldToScreenPoint(result.Tile.transform.position);
                worldToScreen.y = Screen.height - worldToScreen.y - 15f;

                GUI.contentColor = Color.green;
                GUILayout.BeginArea(new Rect(worldToScreen, new Vector2(50f, 50f)), ActualDistances ? result.ActualDistance.ToString() : result.Distance.ToString());
                GUILayout.EndArea();
            }
        }
    }
}
