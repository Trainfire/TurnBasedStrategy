using System.Collections.Generic;
using UnityEngine;

public class Visualizer : MonoBehaviour
{
    private State _state;
    private VisualizerMovePreviewer _movePreviewer;
    private List<TileResult> _tileResults;

    private void Awake()
    {
        _tileResults = new List<TileResult>();
    }

    public void Initialize(State state)
    {
        _state = state;
        _state.Events.PreviewEnabled += OnPreviewShowed;
        _state.Events.PreviewDisabled += OnPreviewCleared;

        _movePreviewer = new VisualizerMovePreviewer(state.Events);
    }

    private void OnPreviewShowed(List<TileResult> tileResults) => _tileResults = tileResults;
    private void OnPreviewCleared() => _tileResults.Clear();

    private void OnGUI()
    {
        foreach (var result in _tileResults)
        {
            if (result.Tile.Blocked)
            {
                result.Tile.Marker.SetNegative();
            }
            else
            {
                result.Tile.Marker.SetPositive();
            }

            // TODO: ???. Move this.
            result.Tile.Marker.DrawText(result.Distance.ToString());
        }
    }
}