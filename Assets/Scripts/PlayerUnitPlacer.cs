using UnityEngine;
using UnityEngine.Assertions;

public class PlayerUnitPlacer : MonoBehaviour
{
    [SerializeField] private Gameboard _gameboard;

    private UnitData _unitData;

    public bool IsPlacingUnit { get; private set; }

    private void Awake()
    {
        Assert.IsNotNull(_gameboard);
    }

    public void BeginPlacingUnit(UnitData unitData)
    {
        Assert.IsNotNull(unitData);

        if (unitData != null)
        {
            IsPlacingUnit = true;
            _unitData = unitData;
        }
    }

    public void PlaceUnit(Tile tile)
    {
        if (!tile.Occupied)
            _gameboard.SpawnUnit(_unitData, tile);

        IsPlacingUnit = false;
    }

    public void CancelPlacingUnit()
    {
        Debug.Log("Cancel unit placement");

        IsPlacingUnit = false;
        _unitData = null;
    }
}
