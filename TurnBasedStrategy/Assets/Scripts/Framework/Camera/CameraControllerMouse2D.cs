using UnityEngine;
using Framework;
using System;

public class CameraControllerMouse2D : MonoBehaviour, IGameCameraController, IInputHandler
{
    [SerializeField] private float _sensitivity = 1f;

    private Vector2 movePosition;

    public float Sensitivity
    {
        get { return _sensitivity; }
        set { _sensitivity = value; }
    }

    public void Awake()
    {
        InputManager.RegisterHandler(this);
    }

    void IGameCameraController.Update(GameCamera gameCamera)
    {
        gameCamera.transform.position += movePosition.ToVec3();
        movePosition = Vector2.zero;
    }

    void IInputHandler.HandleInput(InputActionEvent action)
    {
        if (action.Type == InputActionType.Axis)
        {
            if (action.Action == InputMap.Horizontal)
                movePosition.x += action.Delta * _sensitivity;

            if (action.Action == InputMap.Vertical)
                movePosition.y += action.Delta * _sensitivity;
        }
    }
}
