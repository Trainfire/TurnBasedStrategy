using UnityEngine;

public class WorldParameters
{
    // TODO: To be expanded.
    public Transform Root { get; private set; }
    public GameboardData Data { get; private set; }

    public WorldParameters(Transform root, GameboardData data)
    {
        Root = root;
        Data = data;
    }
}