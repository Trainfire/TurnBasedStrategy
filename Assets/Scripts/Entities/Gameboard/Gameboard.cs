using UnityEngine;
using UnityEngine.Assertions;
using Framework;

public class Gameboard : GameEntity
{
    public GameboardData Data { get { return _data; } }
    public Helper Helper { get; private set; }
    public Entities Entities { get; private set; }
    public IInputEvents InputEvents { get; private set; }
    public State State { get; private set; }

    [SerializeField] private GameboardData _data;

    private void Awake()
    {
        Assert.IsNotNull(Data, "Data is missing.");
        Assert.IsTrue(Data.MaxTurns != 0, "Max Turns is 0!");

        // Temp: Just generate a random world.
        var worldParameters = new WorldParameters(transform, Data.Prefabs.Tile);
        worldParameters.Add(Data.Prefabs.DefaultBuilding);

        var worldGenerator = new WorldGenerator(worldParameters);
        var world = worldGenerator.Generate(Data.GridSize);

        Helper = new Helper(world);
        Entities = new Entities(Helper, world);

        var inputController = gameObject.GetOrAddComponent<InputController>();
        InputEvents = inputController;

        State = gameObject.GetOrAddComponent<State>();
        State.Initialize(this);

        var visualizer = gameObject.GetComponentAssert<Visualizer>();
        visualizer.Initialize(State);

        FindObjectOfType<UIHud>()?.Initialize(this, inputController);
    }
}
