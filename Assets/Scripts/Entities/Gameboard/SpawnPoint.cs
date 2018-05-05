using UnityEngine;
using System;

public class SpawnPoint : MonoBehaviour
{
    public Tile Tile { get; private set; }

    public event Action<SpawnPoint> Spawned;

    public void Initialize(Tile tile)
    {
        Tile = tile;
    }

    public void Spawn()
    {
        if (Tile.Blocked)
            Tile.ApplyHealthChange(-1);

        Spawned?.Invoke(this);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        Tile = null;
        Spawned = null;
    }
}