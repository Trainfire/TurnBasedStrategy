using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Framework;
using Framework.UI;

public class UIHud : MonoBehaviour
{
    private GameboardInput _gameboardInput;
    private GameboardObjects _gameboardObjects;

    public void Initialize(GameboardObjects gameboardObjects, GameboardInput gameboardInput, GameboardState gameboardState)
    {
        gameObject.GetComponent<UIHealthBars>((comp) => comp.Initialize(gameboardObjects));
        gameObject.GetComponent<UIHudActions>((comp) => comp.Initialize(gameboardState, gameboardInput));
    }
}
