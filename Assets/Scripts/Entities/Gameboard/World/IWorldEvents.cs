using System;

public interface IWorldEvents
{
    event Action<Unit> UnitAdded;
    event Action<Unit> UnitRemoved;
    event Action<Tile> TileAdded;
}