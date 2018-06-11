using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class VisualizerTileMovementMarker : MonoBehaviour, IVisualizer
{
    enum TileHighlightType
    {
        None,
        Positive,
        Negative,
    }

    public GameObject PositiveMarker;
    public GameObject NegativeMarker;

    private Tile _tile;
    private string _text;
    private Dictionary<TileHighlightType, GameObject> _markers = new Dictionary<TileHighlightType, GameObject>();
    private TileHighlightType _highlightType;

    public void Initialize(IStateEvents stateEvents)
    {
        stateEvents.PreviewEnabled += OnPreviewEnabled;
        stateEvents.PreviewDisabled += Clear;

        if (PositiveMarker != null)
            _markers.Add(TileHighlightType.Positive, PositiveMarker);

        if (NegativeMarker != null)
            _markers.Add(TileHighlightType.Negative, NegativeMarker);

        _tile = gameObject.GetComponent<Tile>();

        Assert.IsNotNull(_tile);
    }

    private void OnPreviewEnabled(List<TileResult> tileResults)
    {
        foreach (var tileResult in tileResults)
        {
            if (tileResult.Tile == _tile)
            {
                _text = tileResult.Distance.ToString();

                if (tileResult.Tile.Blocked)
                {
                    _highlightType = TileHighlightType.Negative;
                    return;
                }
                else
                {
                    _highlightType = TileHighlightType.Positive;
                    return;
                }
            }
        }

        Clear();
    }

    private void Clear()
    {
        _text = string.Empty;
        _highlightType = TileHighlightType.None;
    }

    private void Update()
    {
        foreach (var kvp in _markers)
        {
            kvp.Value.SetActive(kvp.Key == _highlightType);
        }
    }

    private void OnGUI()
    {
        Debug.DrawLine(transform.position, transform.position + Vector3.up * 2f, Color.green);

        var worldToScreen = Camera.main.WorldToScreenPoint(transform.position);
        worldToScreen.y = Screen.height - worldToScreen.y - 15f;

        GUI.contentColor = Color.green;
        GUILayout.BeginArea(new Rect(worldToScreen, new Vector2(50f, 50f)), _text);
        GUILayout.EndArea();
    }
}
