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

    private Dictionary<TileHighlightType, GameObject> myMarkers;
    private TileHighlightType myHighlightType;

    private void Awake()
    {
        myMarkers = new Dictionary<TileHighlightType, GameObject>();

        if (PositiveMarker != null)
            myMarkers.Add(TileHighlightType.Positive, PositiveMarker);

        if (NegativeMarker != null)
            myMarkers.Add(TileHighlightType.Negative, NegativeMarker);
    }

    private void Update()
    {
        foreach (var kvp in myMarkers)
        {
            kvp.Value.SetActive(kvp.Key == myHighlightType);
        }
    }

    private void LateUpdate()
    {
        myHighlightType = TileHighlightType.None;
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

    public void Clear() { myHighlightType = TileHighlightType.None; }
    public void SetPositive() { myHighlightType = TileHighlightType.Positive; }
    public void SetNegative() { myHighlightType = TileHighlightType.Negative; }
}
