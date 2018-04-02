using UnityEngine;
using UnityEngine.Assertions;

public class TileHazard : MonoBehaviour
{
    protected Tile Tile { get; private set; }
    protected GameboardWorldHelper Helper { get; private set; }

    public virtual void Initialize(Tile tile, GameboardWorldHelper gameboardHelper)
    {
        Assert.IsNotNull(tile);
        Assert.IsNotNull(gameboardHelper);
        Tile = tile;
        Helper = gameboardHelper;
    }
}
