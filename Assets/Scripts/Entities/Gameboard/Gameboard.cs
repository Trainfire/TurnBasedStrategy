using UnityEngine;
using UnityEngine.Assertions;
using Framework;

public class Events
{
    public IInputEvents Input { get; private set; }
    public IStateEvents State { get; private set; }
    public IWorldEvents World { get; private set; }

    public Events(IInputEvents inputEvents, IStateEvents stateEvents, IWorldEvents worldEvents)
    {
        Input = inputEvents;
        State = stateEvents;
        World = worldEvents;
    }
}

public class Gameboard : GameEntity
{
    public GameboardData Data { get { return _data; } }
    public World World { get; private set; }
    public Events Events { get; private set; }
    public IInputEvents InputEvents { get; private set; }
    public State State { get; private set; }

    [SerializeField] private GameboardData _data;

    private void Awake()
    {
        Assert.IsNotNull(Data, "Data is missing.");
        Assert.IsTrue(Data.MaxTurns != 0, "Max Turns is 0!");

        World = new World(new WorldParameters(transform, Data));

        var inputController = gameObject.GetOrAddComponent<InputController>();
        InputEvents = inputController;

        State = GameObject.Instantiate(new GameObject("State"), transform).AddComponent<State>();
        State.Initialize(this);

        var visualizer = gameObject.GetComponentAssert<Visualizer>();
        visualizer.Initialize(this);

        Events = new Events(InputEvents, State.Events, World);

        FindObjectOfType<UIHud>()?.Initialize(this, inputController);

        InputEvents.Kill += InputEvents_Kill;
    }

    private void InputEvents_Kill(Tile obj)
    {
        obj.ApplyHealthChange(-5);
    }
}
