namespace Framework
{
    public interface ILevelObject
    {
        void OnLevelStart();
    }

    /// <summary>
    /// A type of behaviour associated with a Game. Attach to a Game and this will be automatically invoked.
    /// </summary>
    public abstract class GameRule : MonoBehaviourEx
    {
        protected GameController GameController { get; private set; }

        public void Initialize(GameController gameController)
        {
            GameController = gameController;
            OnLevelStart();
        }

        protected virtual void OnLevelStart()
        {
            foreach (var levelObject in InterfaceHelper.FindObjects<ILevelObject>())
            {
                levelObject.OnLevelStart();
            }
        }
    }
}
