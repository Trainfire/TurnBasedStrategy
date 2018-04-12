using UnityEngine;

public class HazardView : MonoBehaviour
{
    private Hazard _hazard;

    public void Initialize(Hazard hazard)
    {
        _hazard = hazard;
        transform.SetGridPosition(hazard.Tile.transform.GetGridPosition());
    }
}