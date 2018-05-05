using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UISpawnPointMarkers : MonoBehaviour
{
    [SerializeField] private GameObject _spawnpointMarkerPrototype;

    private IWorldEvents _worldEvents;
    private Dictionary<SpawnPoint, GameObject> _spawnpointMarkers = new Dictionary<SpawnPoint, GameObject>();

    public void Initialize(IWorldEvents worldEvents)
    {
        _worldEvents = worldEvents;
        _worldEvents.SpawnPointAdded += OnSpawnPointAdded;
    }

    private void OnSpawnPointAdded(SpawnPoint spawnPoint)
    {
        spawnPoint.Spawned += OnSpawnPointSpawned;

        Assert.IsNotNull(_spawnpointMarkerPrototype);

        var view = GameObject.Instantiate(_spawnpointMarkerPrototype);
        view.transform.SetGridPosition(spawnPoint.Tile.transform.GetGridPosition());

        _spawnpointMarkers.Add(spawnPoint, view);
    }

    private void OnSpawnPointSpawned(SpawnPoint spawnPoint)
    {
        Assert.IsTrue(_spawnpointMarkers.ContainsKey(spawnPoint));

        if (_spawnpointMarkers.ContainsKey(spawnPoint))
        {
            Destroy(_spawnpointMarkers[spawnPoint]);
            _spawnpointMarkers.Remove(spawnPoint);
        }

        spawnPoint.Spawned -= OnSpawnPointSpawned;
    }
}
