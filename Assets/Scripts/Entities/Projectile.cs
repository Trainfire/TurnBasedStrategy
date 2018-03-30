using UnityEngine;
using System;
using Framework;

public struct ProjectileCollisionEvent
{
    public Collider Collider { get; private set; }
    public WorldDirection Direction { get; private set; }

    public ProjectileCollisionEvent(Collider collider, WorldDirection direction)
    {
        Collider = collider;
        Direction = direction;
    }
}

public class Projectile : MonoBehaviour
{
    [SerializeField] private float _speed;

    private Action<ProjectileCollisionEvent> _callback;
    private WorldDirection _moveDirection;

    public void ApplyForce(WorldDirection moveDirection, Action<ProjectileCollisionEvent> callback = null)
    {
        _moveDirection = moveDirection;
        _callback = callback;
    }

    private void Update()
    {
        if (GameboardHelper.OutOfBounds(transform.GetGridPosition()))
        {
            Destroy(gameObject);
            return;
        }

        transform.SetGridPosition(transform.GetGridPosition() + GridHelper.DirectionToVector(_moveDirection) * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_callback != null)
            _callback.InvokeSafe(new ProjectileCollisionEvent(other, _moveDirection));
    }
}