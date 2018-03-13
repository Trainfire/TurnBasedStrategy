using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using Framework;

public class ZoneManager<T>
{
    private IZoneHandler<T> _handler;
    private List<string> _activeScenes;
    private SceneLoader _sceneLoader;

    public ZoneListener<T> Listener { get; private set; }
    public T Zone { get; private set; }
    public string ActiveScene
    {
        get { return SceneManager.GetActiveScene().name; }
    }

    public ZoneManager(SceneLoader sceneLoader)
    {
        Listener = new ZoneListener<T>();
        _activeScenes = new List<string>();
        _handler = Listener;

        _sceneLoader = sceneLoader;
        _sceneLoader.LoadProgress += _handler.OnZoneLoadProgress;
    }

    public void SetZone(T zone, params string[] sceneNames)
    {
        _handler.OnZoneChanging();

        _sceneLoader.StartCoroutine(_sceneLoader.Load(_activeScenes.ToArray(), sceneNames, () =>
        {
            // Level has finished loading. Let's finish up...
            Zone = zone;
            _handler.OnZoneChanged(zone);

            // Keep track of which scenes were loaded.
            _activeScenes.Clear();
            _activeScenes.AddRange(sceneNames);
        }));
    }
}

public interface IZoneHandler<T>
{
    void OnZoneChanging();
    void OnZoneLoadProgress(float progress);
    void OnZoneChanged(T zone);
}

public class ZoneListener<T> : IZoneHandler<T>
{
    public event Action ZoneChanging;
    public event Action<float> ZoneLoadProgress;
    public event Action<T> ZoneChanged;

    void IZoneHandler<T>.OnZoneChanged(T zone)
    {
        ZoneChanged.InvokeSafe(zone);
    }

    void IZoneHandler<T>.OnZoneChanging()
    {
        ZoneChanging.InvokeSafe();
    }

    void IZoneHandler<T>.OnZoneLoadProgress(float progress)
    {
        ZoneLoadProgress.InvokeSafe(progress);
    }
}
