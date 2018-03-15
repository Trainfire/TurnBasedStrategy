using UnityEngine;
using System;
using Framework;

public class Unit : MonoBehaviour
{
    public event Action<Unit> Moved;

    public void Move(Tile tile)
    {
        if (tile.Occupied)
        {
            Debug.LogWarningFormat("Unit {0} cannot move to tile {1} as it is occupied.", name, tile.name);
            return;
        }

        transform.position = tile.transform.position;
        Moved.InvokeSafe(this);
        tile.SetOccupant(this);
    }
}
