using UnityEngine;
using System;
using System.Collections.Generic;

namespace Framework
{
    public class AreaTrigger2DEvent
    {
        public AreaTrigger2D Sender { get; private set; }
        public Collider2D Collider { get; private set; }

        public AreaTrigger2DEvent(AreaTrigger2D sender, Collider2D collider)
        {
            Sender = sender;
            Collider = collider;
        }
    }

    /// <summary>
    /// Exposes the OnTriggerEnter2D and OnTriggerExit2D Monobehaviours as events.
    /// </summary>
    public class AreaTrigger2D : MonoBehaviour
    {
        public event Action<AreaTrigger2DEvent> Entered;
        public event Action<AreaTrigger2DEvent> Left;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            Entered.InvokeSafe(new AreaTrigger2DEvent(this, collider));
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            Left.InvokeSafe(new AreaTrigger2DEvent(this, collider));
        }
    }
}