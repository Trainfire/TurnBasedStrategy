using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class VisualizerTileHoverMarker : MonoBehaviour, IInputVisualizer
{
    [SerializeField] private GameObject _marker;

    private Tile _tile;

    public void Initialize(IInputEvents inputEvents)
    {
        inputEvents.HoveredTileChanged += OnHoveredTileChanged;

        _tile = gameObject.GetComponent<Tile>();

        Assert.IsNotNull(_tile);
        Assert.IsNotNull(_marker);

        _marker.SetActive(false);
    }

    private void OnHoveredTileChanged(Tile hoveredTile)
    {
        _marker.SetActive(hoveredTile == _tile);
    }
}
