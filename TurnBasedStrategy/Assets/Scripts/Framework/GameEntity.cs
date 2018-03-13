namespace Framework
{
    public interface IGameEntity
    {
        bool Initialized { get; }
        void Initialize(Game game);
    }

    /// <summary>
    /// Represents a class dependant on the Game. The Initialize method is called after MonoBehaviour's Start.
    /// </summary>
    public abstract class GameEntity : MonoBehaviourEx, IGameEntity, IInputHandler
    {
        protected Game Game { get; private set; }

        public int InstanceID
        {
            get { return GetInstanceID(); }
        }

        private bool _initialized;
        bool IGameEntity.Initialized
        {
            get { return _initialized; }
        }

        void IGameEntity.Initialize(Game game)
        {
            _initialized = true;

            Game = game;
            Game.StateListener.StateChanged += OnStateChanged;

            InputManager.RegisterHandler(this);

            OnInitialize();
        }

        protected virtual void OnInitialize() { }
        protected virtual void OnStateChanged(State state) { }
        public virtual void HandleInput(InputActionEvent action) { }

        /// <summary>
        /// Called when the GameObject is destroyed. If override, you must call the base method!
        /// </summary>
        protected virtual void OnDestroy()
        {
            InputManager.UnregisterHandler(this);

            if (Game != null)
                Game.StateListener.StateChanged -= OnStateChanged;
        }
    }
}
