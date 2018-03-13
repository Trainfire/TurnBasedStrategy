﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameboardTile : MonoBehaviour
{
    public bool Occupied { get { return myOccupant != null; } }
    public Unit Occupant { get { return myOccupant; } }

    private Unit myOccupant;

    /// <summary>
    /// Sets the occupant.
    /// </summary>
    /// <param name="occupant">Set to null to clear the occupant.</param>
    public void SetOccupant(Unit occupant)
    {
        if (myOccupant != null)
            myOccupant.Moved -= MyOccupant_Moved;

        myOccupant = occupant;

        Debug.LogFormat("{0} is now occupied by {1}", name, GetOccupantName());

        if (myOccupant != null)
            myOccupant.Moved += MyOccupant_Moved;
    }

    string GetOccupantName()
    {
        return myOccupant != null ? myOccupant.name : "Nobody";
    }

    private void MyOccupant_Moved(Unit unit)
    {
        Debug.LogFormat("Occupant {0} vacated from {1} ", GetOccupantName(), name);
        SetOccupant(null);
    }
}
