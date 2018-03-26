using UnityEngine;
using UnityEditor;
using System.Collections.ObjectModel;
using System.Collections.Generic;

public static class GridHelper
{
    public static Vector2 TransformToGridspace(this Vector3 v) { return new Vector2(v.x, v.z); }
    public static Vector3 TransformFromGridspace(this Vector2 v) { return new Vector3(v.x, 0f, v.y); }

    public static readonly IEnumerable<WorldDirection> AllDirections = new List<WorldDirection>()
    {
        WorldDirection.North,
        WorldDirection.East,
        WorldDirection.South,
        WorldDirection.West,
    };

    public static Vector2 DirectionToVector(WorldDirection direction)
    {
        switch (direction)
        {
            case WorldDirection.North: return Vector2.up;
            case WorldDirection.East: return Vector2.right;
            case WorldDirection.South: return Vector2.down;
            case WorldDirection.West: return Vector2.left;
        }

        return Vector2.zero;
    }

    public static WorldDirection VectorToDirection(Vector2 direction)
    {
        if (direction == Vector2.up)
        {
            return WorldDirection.North;
        }
        else if (direction == Vector2.right)
        {
            return WorldDirection.East;
        }
        else if (direction == Vector2.down)
        {
            return WorldDirection.South;
        }

        return WorldDirection.West;
    }
}