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
    //public Helper Helper { get; private set; }
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

        //Helper = new Helper(world);
        //Entities = new Entities(Helper, world);

        var inputController = gameObject.GetOrAddComponent<InputController>();
        InputEvents = inputController;

        State = gameObject.GetOrAddComponent<State>();
        State.Initialize(this);

        var visualizer = gameObject.GetComponentAssert<Visualizer>();
        visualizer.Initialize(State);

        Events = new Events(InputEvents, State.Events, World);

        FindObjectOfType<UIHud>()?.Initialize(this, inputController);
    }
}
