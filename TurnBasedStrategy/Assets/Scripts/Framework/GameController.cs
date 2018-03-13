using UnityEngine;

namespace Framework
{
    public class GameController : IInputHandler
    {
        private StateManager _stateManager;
        private ZoneManager<GameZone> _zoneManager;

        public Game Game { get; private set; }

        public GameController(Game game, StateManager stateManager, ZoneManager<GameZone> zoneManager)
        {
            Game = game;
            _stateManager = stateManager;
            _zoneManager = zoneManager;

            InputManager.RegisterHandler(this);
        }

        public void StartGame()
        {
            LoadLevel("level");
        }

        public void Resume()
        {
            _stateManager.SetState(State.Running);
        }

        public void Pause()
        {
            _stateManager.SetState(State.Paused);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

        public void LoadMainMenu()
        {
            _zoneManager.SetZone(GameZone.MainMenu, "MainMenu");
        }

        public void LoadLevel(string sceneName)
        {
            // Find a bootstrapper in the scene and remove it.
            var bootstrapper = GameObject.FindObjectOfType<Bootstrapper>();
            if (bootstrapper != null)
                GameObject.Destroy(bootstrapper);

            _zoneManager.SetZone(GameZone.InGame, "InGame", sceneName);
        }

        public void ReloadLevel()
        {
            _zoneManager.SetZone(GameZone.InGame, "InGame", _zoneManager.ActiveScene);
        }

        void IInputHandler.HandleInput(InputActionEvent action)
        {
            if (action.Action == InputMap.Start && action.Type == InputActionType.Down)
            {
                _stateManager.ToggleState();
                Debug.Log("Game is now " + _stateManager.State);
            }
        }
    }
}
