using UnityEngine;
using UnityEngine.Assertions;

public class TileHazard : MonoBehaviour
{
    protected Tile Tile { get; private set; }
    protected GameboardHelper Helper { get; private set; }

    public virtual void Initialize(Tile tile, GameboardHelper gameboardHelper)
    {
        Assert.IsNotNull(tile);
        Assert.IsNotNull(gameboardHelper);
        Tile = tile;
        Helper = gameboardHelper;
    }
}
