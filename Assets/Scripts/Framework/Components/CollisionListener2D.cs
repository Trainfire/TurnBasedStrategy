using UnityEngine;
using System;

namespace Framework
{
    public class CollisionListener2DEvent
    {
        public CollisionListener2D Sender { get; private set; }
        public Collision2D Collider { get; private set; }

        public CollisionListener2DEvent(CollisionListener2D sender, Collision2D collider)
        {
            Sender = sender;
            Collider = collider;
        }
    }

    /// <summary>
    /// Exposes the OnTriggerEnter2D and OnTriggerExit2D Monobehaviours as events.
    /// </summary>
    public class CollisionListener2D : MonoBehaviour
    {
        public event Action<CollisionListener2DEvent> Entered;
        public event Action<CollisionListener2DEvent> Left;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Entered.InvokeSafe(new CollisionListener2DEvent(this, collision));
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            Left.InvokeSafe(new CollisionListener2DEvent(this, collision));
        }
    }
}
