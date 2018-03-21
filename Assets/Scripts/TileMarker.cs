using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMarker : MonoBehaviour
{
    enum TileHighlightType
    {
        None,
        Positive,
        Negative,
    }

    public GameObject PositiveMarker;
    public GameObject NegativeMarker;

    private Dictionary<TileHighlightType, GameObject> _markers;
    private TileHighlightType _highlightType;

    private void Awake()
    {
        _markers = new Dictionary<TileHighlightType, GameObject>();

        if (PositiveMarker != null)
            _markers.Add(TileHighlightType.Positive, PositiveMarker);

        if (NegativeMarker != null)
            _markers.Add(TileHighlightType.Negative, NegativeMarker);
    }

    private void Update()
    {
        foreach (var kvp in _markers)
        {
            kvp.Value.SetActive(kvp.Key == _highlightType);
        }
    }

    private void LateUpdate()
    {
        _highlightType = TileHighlightType.None;
    }

    public void DrawText(string text)
    {
        Debug.DrawLine(transform.position, transform.position + Vector3.up * 2f, Color.green);

        var worldToScreen = Camera.main.WorldToScreenPoint(transform.position);
        worldToScreen.y = Screen.height - worldToScreen.y - 15f;

        GUI.contentColor = Color.green;
        GUILayout.BeginArea(new Rect(worldToScreen, new Vector2(50f, 50f)), text);
        GUILayout.EndArea();
    }

    public void Clear() { _highlightType = TileHighlightType.None; }
    public void SetPositive() { _highlightType = TileHighlightType.Positive; }
    public void SetNegative() { _highlightType = TileHighlightType.Negative; }
}
