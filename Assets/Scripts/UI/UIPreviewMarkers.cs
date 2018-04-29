using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UIPreviewMarkers : MonoBehaviour
{
    [SerializeField] private GameObject _pushMarkerPrototype;

    private IStateEvents _stateEvents;
    private List<GameObject> _pushMarkers = new List<GameObject>();

    public void Initialize(IStateEvents stateEvents)
    {
        _stateEvents = stateEvents;

        _stateEvents.EffectPreviewShow += ShowMarkers;
        _stateEvents.PreviewDisabled += ClearMarkers;
    }

    private void ShowMarkers(EffectPreview effectPreview)
    {
        ClearMarkers();

        foreach (var item in effectPreview.Pushes)
        {
            var instance = SpawnPushMarker(item.Key, item.Value);
            _pushMarkers.Add(instance);
        }
    }

    private void ClearMarkers()
    {
        _pushMarkers.ForEach(x => Destroy(x));
        _pushMarkers.Clear();
    }

    private GameObject SpawnPushMarker(Tile tile, WorldDirection worldDirection)
    {
        Assert.IsNotNull(_pushMarkerPrototype);

        var instance = GameObject.Instantiate(_pushMarkerPrototype);

        instance.transform.SetGridPosition(tile.transform.GetGridPosition());
        instance.transform.position += Vector3.up * 0.01f;
        instance.transform.rotation = GridHelper.DirectionToQuaternion(worldDirection);

        return instance;
    }
}
