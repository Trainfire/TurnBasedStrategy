using UnityEngine;
using UnityEditor;
using System.Collections.ObjectModel;
using System.Collections.Generic;

public static class GridHelper
{
    public static Vector2 TransformToGridspace(this Vector3 v) { return new Vector2(v.x, v.z); }
    public static Vector3 TransformFromGridspace(this Vector2 v) { return new Vector3(v.x, 0f, v.y); }

    public static readonly IEnumerable<GameboardDirection> AllDirections = new List<GameboardDirection>()
    {
        GameboardDirection.North,
        GameboardDirection.East,
        GameboardDirection.South,
        GameboardDirection.West,
    };

    public static Vector2 GetVectorFromDirection(GameboardDirection direction)
    {
        switch (direction)
        {
            case GameboardDirection.North: return Vector2.up;
            case GameboardDirection.East: return Vector2.right;
            case GameboardDirection.South: return Vector2.down;
            case GameboardDirection.West: return Vector2.left;
        }

        return Vector2.zero;
    }
}