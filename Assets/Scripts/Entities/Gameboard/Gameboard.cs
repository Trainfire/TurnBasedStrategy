using UnityEngine;
using System.Collections.Generic;
using Framework;

public class Gameboard : GameEntity
{
    public static int GridSize { get; private set; }

    public GameboardWorldHelper Helper { get; private set; }
    public GameboardObjects Objects { get; private set; }
    public GameboardInput Input { get; private set; }
    public GameboardVisualizer Visualizer { get; private set; }
    public GameboardState State { get; private set; }
    public Player Player { get; private set; }

    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Building _buildingPrefab;
    [SerializeField] private int _gridSize;

    private void Awake()
    {
        GridSize = _gridSize;

        // Temp: Just generate a random world.
        var worldParameters = new GameboardWorldParameters(transform, _tilePrefab);
        worldParameters.Add(_buildingPrefab);

        var worldGenerator = new GameboardWorldGenerator(worldParameters);
        var world = worldGenerator.Generate(_gridSize);

        Helper = new GameboardWorldHelper(world);
        Objects = new GameboardObjects(Helper, world);
        Input = gameObject.GetOrAddComponent<GameboardInput>();

        // TODO: Make this a regular class. Not a MonoBehaviour.
        Visualizer = gameObject.GetComponentAssert<GameboardVisualizer>();
        Visualizer.Initialize(this);

        State = new GameboardState(Objects, Input);

        Player = FindObjectOfType<Player>();
        Player.Initialize(this);
    }
}
