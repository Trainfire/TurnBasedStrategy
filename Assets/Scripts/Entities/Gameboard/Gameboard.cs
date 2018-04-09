using UnityEngine;
using System.Collections.Generic;
using Framework;

public class Gameboard : GameEntity
{
    public static int GridSize { get; private set; }
    public static MechData DefaultMech { get; private set; }

    public GameboardWorldHelper Helper { get; private set; }
    public GameboardObjects Objects { get; private set; }
    public IGameboardInputEvents InputEvents { get; private set; }
    public GameboardVisualizer Visualizer { get; private set; }
    public GameboardState State { get; private set; }

    [SerializeField] private MechData _defaultMech;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Building _buildingPrefab;
    [SerializeField] private int _gridSize;

    private void Awake()
    {
        GridSize = _gridSize;
        DefaultMech = _defaultMech;

        // Temp: Just generate a random world.
        var worldParameters = new GameboardWorldParameters(transform, _tilePrefab);
        worldParameters.Add(_buildingPrefab);

        var worldGenerator = new GameboardWorldGenerator(worldParameters);
        var world = worldGenerator.Generate(_gridSize);

        Helper = new GameboardWorldHelper(world);
        Objects = new GameboardObjects(Helper, world);

        var input = gameObject.GetOrAddComponent<GameboardInput>();
        InputEvents = input;

        // TODO: Make this a regular class. Not a MonoBehaviour.
        Visualizer = gameObject.GetComponentAssert<GameboardVisualizer>();
        Visualizer.Initialize(this);

        State = new GameboardState(this);

        var uiHud = FindObjectOfType<UIHud>();
        if (uiHud != null)
            uiHud.Initialize(Objects, input, State);
    }
}
