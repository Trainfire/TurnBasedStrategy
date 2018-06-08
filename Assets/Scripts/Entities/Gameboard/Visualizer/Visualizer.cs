using System.Collections.Generic;
using UnityEngine;
using Framework;

public interface IVisualizer
{
    void Initialize(IStateEvents stateEvents);
}

public class Visualizer : MonoBehaviour
{
    private List<IVisualizer> _visualizers = new List<IVisualizer>();

    public void Initialize(Gameboard gameboard)
    {
        foreach (var tile in gameboard.World.Tiles)
        {
            var tileMarkerComponent = tile.Value.gameObject.GetComponent<VisualizerTileMarker>();
            if (tileMarkerComponent != null)
            {
                _visualizers.Add(tileMarkerComponent);
            }
            else
            {
                DebugEx.LogWarning<Visualizer>("Missing '{0}' component on '{1}'", typeof(VisualizerTileMarker).Name, tile.Value);
            }
        }

        _visualizers.Add(new VisualizerMovePreviewer());

        _visualizers.ForEach(x => x.Initialize(gameboard.State.Events));
    }

    private void OnDestroy()
    {
        _visualizers.Clear();
    }
}