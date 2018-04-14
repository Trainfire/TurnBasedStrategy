using UnityEngine;

public class HazardView : MonoBehaviour
{
    private Hazard _hazard;

    public void Initialize(Hazard hazard)
    {
        _hazard = hazard;
        _hazard.Removed += OnHazardRemove;

        transform.SetGridPosition(hazard.Tile.transform.GetGridPosition());
    }

    private void OnHazardRemove(Hazard hazard)
    {
        _hazard = null;

        Destroy(gameObject);
    }
}