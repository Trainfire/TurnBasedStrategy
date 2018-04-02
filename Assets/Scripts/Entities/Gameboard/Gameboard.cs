using UnityEngine;
using Framework;

public class Gameboard : GameEntity
{
    public static int GridSize { get; private set; }

    public GameboardWorldHelper Helper { get; private set; }
    public GameboardVisualizer Visualizer { get; private set; }
    public GameboardObjects Objects { get; private set; }

    [SerializeField] private Tile _prefab;
    [SerializeField] private int _gridSize;

    private void Awake()
    {
        GridSize = _gridSize;

        // Temp: Just generate a random world.
        var worldGenerator = new GameboardWorldGenerator(transform, _prefab);
        var world = worldGenerator.Generate(_gridSize);

        Helper = new GameboardWorldHelper(world);

        // TODO: Make this a regular class. Not a MonoBehaviour.
        Visualizer = gameObject.GetComponentAssert<GameboardVisualizer>();
        Visualizer.Initialize(this);

        Objects = new GameboardObjects(Helper);

        var player = FindObjectOfType<Player>();
        player.Initialize(this);
    }
}
