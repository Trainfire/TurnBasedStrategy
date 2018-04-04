using UnityEngine;
using System.Collections.Generic;

namespace Framework
{
    public enum GameZone
    {
        None,
        Loading,
        MainMenu,
        InGame,
    }

    public abstract class Game : MonoBehaviour
    {
        private ConsoleController _console;
        private List<GameRule> _rules;

        private SceneLoader _sceneLoader;
        protected SceneLoader SceneLoader
        {
            get { return _sceneLoader; }
        }

        private GameController _controller;
        public GameController Controller
        {
            get { return _controller; }
        }

        private StateManager _stateManager;
        public StateListener StateListener
        {
            get { return _stateManager.Listener; }
        }

        private ZoneManager<GameZone> _zoneManager;
        public ZoneListener<GameZone> ZoneListener
        {
            get { return _zoneManager.Listener; }
        }

        public bool Initialized { get; private set; }
        public GameCamera Camera { get; private set; }

        public void Initialize(params string[] args)
        {
            if (Initialized)
            {
                Debug.LogError("Game has already been initialized. This should not happen!");
                return;
            }

            Initialized = true;

            // Console
            var consoleView = FindObjectOfType<ConsoleView>();
            if (consoleView == null)
            {
                Debug.LogWarning("Failed to find a ConsoleView! The Console will be unavailable!");
            }
            else
            {
                _console = new ConsoleController();
                consoleView.SetConsole(_console);
            }

            // Relay
            gameObject.GetOrAddComponent<MonoEventRelay>();

            // State
            _stateManager = new StateManager();
            _stateManager.Listener.StateChanged += Listener_StateChanged;

            // Scene Loader
            _sceneLoader = gameObject.GetOrAddComponent<SceneLoader>();

            // Zone
            _zoneManager = new ZoneManager<GameZone>(_sceneLoader);
            _zoneManager.Listener.ZoneChanged += Listener_ZoneChanged;

            // Allows the control of the game, such as level loading, resuming and pausing the game.
            _controller = new GameController(this, _stateManager, _zoneManager);

            // Audio
            Services.Provide(FindObjectOfType<AudioSystem>());

            // Game Entities
            new GameEntityManager(this);

            // Rules
            _rules = new List<GameRule>();

            // Game implementation
            OnInitialize(args);
        }

        protected virtual void OnInitialize(params string[] args) { }

        protected virtual void RegisterRule<T>() where T : GameRule
        {
            var rule = gameObject.GetOrAddComponent<T>();
            _rules.Add(rule);
        }

        private void Listener_StateChanged(State state)
        {
            // Resume / Pause Game
            Time.timeScale = state == State.Running ? 1f : 0f;
        }

        private void Listener_ZoneChanged(GameZone gameZone)
        {
            _stateManager.SetState(State.Running);

            if (gameZone == GameZone.InGame)
            {
                _rules.ForEach(x => x.Initialize(_controller));
            }
        }
    }

    class GameEntityManager
    {
        private Game _game;
        private List<GameEntity> _entities;

        public GameEntityManager(Game game)
        {
            _game = game;
            _game.ZoneListener.ZoneChanged += ZoneListener_ZoneChanged;
        }

        private void ZoneListener_ZoneChanged(GameZone gameZone)
        {
            foreach (var entity in InterfaceHelper.FindObjects<IGameEntity>())
            {
                if (!entity.Initialized)
                    entity.Initialize(_game);
            }
        }
    }
}
