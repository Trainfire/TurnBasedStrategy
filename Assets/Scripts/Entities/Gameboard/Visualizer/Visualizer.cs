using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
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
            RegisterTileComponent<VisualizerTileMovementMarker>(tile.Value);
            RegisterTileComponent<VisualizerTileTargetMarker>(tile.Value);
        }

        _visualizers.Add(new VisualizerMovePreviewer());

        _visualizers.ForEach(x => x.Initialize(gameboard.State.Events));
    }

    private void RegisterTileComponent<T>(Tile tile) where T : IVisualizer
    {
        var component = tile.gameObject.GetComponent<T>();
        if (component != null)
        {
            _visualizers.Add(component);
        }
        else
        {
            DebugEx.LogWarning<Visualizer>("Missing '{0}' component on '{1}'", typeof(T).Name, tile);
        }
    }

    private void OnDestroy()
    {
        _visualizers.Clear();
    }
}