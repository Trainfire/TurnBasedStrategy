using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Framework;

public interface IStateVisualizer
{
    void Initialize(IStateEvents stateEvents);
}

public interface IInputVisualizer
{
    void Initialize(IInputEvents inputEvents);
}

public class Visualizer : MonoBehaviour
{
    private List<IStateVisualizer> _stateVisualizers = new List<IStateVisualizer>();
    private List<IInputVisualizer> _inputVisualizers = new List<IInputVisualizer>();

    public void Initialize(Gameboard gameboard)
    {
        foreach (var tile in gameboard.World.Tiles)
        {
            RegisterVisualizer<IStateVisualizer, VisualizerTileMovementMarker>(_stateVisualizers, tile.Value);
            RegisterVisualizer<IStateVisualizer, VisualizerTileTargetMarker>(_stateVisualizers, tile.Value);
            RegisterVisualizer<IInputVisualizer, VisualizerTileHoverMarker>(_inputVisualizers, tile.Value);
        }

        _stateVisualizers.Add(new VisualizerMovePreviewer());

        _stateVisualizers.ForEach(x => x.Initialize(gameboard.State.Events));
        _inputVisualizers.ForEach(x => x.Initialize(gameboard.InputEvents));
    }

    private void RegisterVisualizer<TList, TComponent>(List<TList> list, Tile tile) where TComponent : TList
    {
        var component = tile.gameObject.GetComponent<TComponent>();
        if (component != null)
        {
            list.Add(component);
        }
        else
        {
            DebugEx.LogWarning<Visualizer>("Missing '{0}' component on '{1}'", typeof(TComponent).Name, tile);
        }
    }

    private void OnDestroy()
    {
        _stateVisualizers.Clear();
        _inputVisualizers.Clear();
    }
}