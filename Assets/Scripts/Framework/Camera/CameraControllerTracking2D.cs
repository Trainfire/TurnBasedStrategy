using UnityEngine;
using Framework;

public class CameraControllerTracking2D : MonoBehaviour, IGameCameraController
{
    [Range(0f, 1f)] [SerializeField] private float _moveSpeed;
    [SerializeField] private Vector2 _offset;

    private GameObject _trackingTarget;
    private Vector2 _worldToScreen;
    private Vector2 _lerp;

    public Transform Target
    {
        get { return _trackingTarget.transform; }
    }

    public void SetTarget(GameObject target)
    {
        _trackingTarget = target;
    }

    void IGameCameraController.Update(GameCamera gameCamera)
    {
        if (_trackingTarget == null)
            return;

        _moveSpeed = Mathf.Clamp(_moveSpeed, 0f, 1f);

        // Get target in screen space and centralise.
        _worldToScreen = gameCamera.Camera.WorldToScreenPoint(_trackingTarget.transform.position).ToVec2();
        _worldToScreen += new Vector2(-Screen.width / 2f, -Screen.height / 2f) + _offset;

        _lerp = Vector2.Lerp(gameCamera.transform.position, _worldToScreen, Time.deltaTime * _moveSpeed);

        // Set position ignoring Z.
        gameCamera.transform.position = new Vector3(_lerp.x, _lerp.y, gameCamera.transform.position.z);
    }
}
