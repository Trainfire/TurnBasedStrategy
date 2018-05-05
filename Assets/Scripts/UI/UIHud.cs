using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Framework;
using Framework.UI;

public class UIHud : MonoBehaviour
{
    public void Initialize(Gameboard gameboard, InputController inputController)
    {
        gameObject.GetComponent<UIHealthBars>((comp) => comp.Initialize(gameboard));
        gameObject.GetComponent<UIHudActions>((comp) => comp.Initialize(gameboard, inputController));
        gameObject.GetComponent<UIPreviewMarkers>((comp) => comp.Initialize(gameboard.Events.State));
        gameObject.GetComponent<UISpawnPointMarkers>((comp) => comp.Initialize(gameboard.Events.World));
    }
}
