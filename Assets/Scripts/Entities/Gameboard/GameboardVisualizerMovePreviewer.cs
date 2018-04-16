using UnityEngine;
using System.Collections;

public class GameboardVisualizerMovePreviewer : MonoBehaviour
{
    private GameObject _unitPreviewInstance;

    private void OnActionSetToMove(Mech mech)
    {
        _unitPreviewInstance = GameObject.Instantiate(mech.MechData.View);
        _unitPreviewInstance.transform.SetGridPosition(mech.transform.GetGridPosition());
    }

    private void SetPosition(Tile targetTile)
    {
        if (_unitPreviewInstance == null || targetTile == null)
            return;

        _unitPreviewInstance.transform.SetGridPosition(targetTile.transform.GetGridPosition());
    }

    private void Clear()
    {
        GameObject.Destroy(_unitPreviewInstance);
    }
}