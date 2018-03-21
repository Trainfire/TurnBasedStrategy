using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class Tile : MonoBehaviour
{
    public TileMarker Marker { get; private set; }

    public Vector2 Position { get { return transform.position.TransformToGridspace(); } }
    public bool Occupied { get { return _occupant != null; } }
    public Unit Occupant { get { return _occupant; } }

    private string OccupantName { get { return _occupant != null ? _occupant.name : "Nobody"; } }

    private Unit _occupant;

    private void Awake()
    {
        Marker = gameObject.GetOrAddComponent<TileMarker>();
    }

    /// <summary>
    /// Sets the occupant.
    /// </summary>
    /// <param name="occupant">Set to null to clear the occupant.</param>
    public void SetOccupant(Unit occupant)
    {
        if (_occupant != null)
        {
            _occupant.Died -= OnOccupantDeath;
            _occupant.Moved -= OnOccupantMoved;
        }

        _occupant = occupant;

        Debug.LogFormat("{0} is now occupied by {1}", name, OccupantName);

        if (_occupant != null)
        {
            _occupant.Died += OnOccupantDeath;
            _occupant.Moved += OnOccupantMoved;
            _occupant.transform.position = Position.TransformFromGridspace();
        }
    }

    private void OnOccupantDeath(Unit unit)
    {
        SetOccupant(null);
    }

    private void OnOccupantMoved(UnitMoveEvent unitMoveEvent)
    {
        Debug.LogFormat("Occupant {0} vacated from {1} ", OccupantName, name);
        SetOccupant(null);
    }
}
