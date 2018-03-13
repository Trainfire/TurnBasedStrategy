using UnityEngine;
using UnityEngine.Assertions;

public class UnitPlacementResult
{
    public Unit Unit { get; private set; }
    public GameboardTile Tile { get; private set; }

    public UnitPlacementResult(Unit unit, GameboardTile tile)
    {
        Unit = unit;
        Tile = tile;
    }
}

public class PlayerUnitPlacer : MonoBehaviour
{
    private Unit myUnit;

    public bool IsPlacingUnit { get; private set; }

    public void BeginPlacingUnit(Unit unitPrototype)
    {
        Assert.IsNotNull(unitPrototype);

        if (unitPrototype != null)
        {
            IsPlacingUnit = true;
            myUnit = GameObject.Instantiate(unitPrototype);
        }
    }

    public UnitPlacementResult PlaceUnit(GameboardTile tile)
    {
        if (tile != null && !tile.Occupied)
        {
            var placementData = new UnitPlacementResult(myUnit, tile);

            tile.SetOccupant(placementData.Unit);

            myUnit = null;
            IsPlacingUnit = false;

            return placementData;
        }

        Debug.LogWarning("Cannot place unit in occupied or null space");

        return null;
    }

    public void CancelPlacingUnit()
    {
        Debug.Log("Cancel unit placement");

        IsPlacingUnit = false;

        if (myUnit != null)
        {
            Destroy(myUnit.gameObject);
            myUnit = null;
        }
    }

    void Update()
    {
        if (IsPlacingUnit && myUnit != null)
        {
            var myTileUnderMouse = Player.GetTileUnderMouse();

            myUnit.transform.position = myTileUnderMouse != null ? myTileUnderMouse.transform.position : Vector3.zero;
            myUnit.gameObject.SetActive(myTileUnderMouse != null);
        }
    }
}
