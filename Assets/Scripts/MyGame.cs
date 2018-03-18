using UnityEngine;
using Framework;

public class MyGame : Game
{
    protected override void OnInitialize(params string[] args)
    {
        base.OnInitialize(args);

        // Set loading scene.
        SceneLoader.LoadingScene = "Loader";

        // Determines where to go first.
        if (args != null && args.Length != 0)
        {
            Controller.LoadLevel(args[0]);
        }
        else
        {
            Controller.LoadMainMenu();
        }
    }
}